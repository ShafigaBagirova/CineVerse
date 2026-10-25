using Application.Common.Interfaces;
using Application.WatchRooms.Commands;
using Application.WatchRooms.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace API.Hubs;

public sealed record WatchRoomSnapshot(
    GetWatchRoomByIdResponse Room,
    WatchRoomPlaybackState Playback,
    IReadOnlyCollection<string> OnlineUserIds);

[Authorize]
public class WatchRoomHub : Hub
{
    private const string JoinedRoomsKey = "watchroom:joined";
    private const string UserNameKey = "watchroom:userName";
    private const int MaxMessageLength = 500;
    private const int MaxReactionLength = 8;
    private const string RateLimitKey = "watchroom:rateLimit";
    private const int MaxMessagesPerWindow = 10;
    private const int MaxReactionsPerWindow = 20;
    private const int MaxSyncCallsPerWindow = 30;
    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan RecentActionWindow = TimeSpan.FromSeconds(3);

    private readonly IMediator _mediator;
    private readonly IWatchRoomStateStore _stateStore;
    private readonly ILogger<WatchRoomHub> _logger;

    public WatchRoomHub(IMediator mediator, IWatchRoomStateStore stateStore, ILogger<WatchRoomHub> logger)
    {
        _mediator = mediator;
        _stateStore = stateStore;
        _logger = logger;
    }

    public async Task<WatchRoomSnapshot> JoinRoom(string code)
    {
        var userId = GetUserId();
        var result = await _mediator.Send(new JoinWatchRoomCommand(code));
        if (!result.Success || result.Data is null)
            throw new HubException(result.Message ?? "Failed to join watch room.");

        var room = result.Data;
        var roomCode = room.Code;
        var userName = room.Members.FirstOrDefault(m => m.UserId == userId)?.UserName;

        Context.Items[UserNameKey] = userName;
        JoinedRooms()[roomCode] = room.HostUserId;

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(roomCode));
        _stateStore.AddConnection(roomCode, Context.ConnectionId, userId);

        await Clients.OthersInGroup(GroupName(roomCode)).SendAsync("MemberJoined", new { UserId = userId, UserName = userName });

        _logger.LogInformation(
            "Watch room connection joined. Code: {Code}, UserId: {UserId}, ConnectionId: {ConnectionId}, TimestampUtc: {TimestampUtc}",
            roomCode,
            userId,
            Context.ConnectionId,
            DateTime.UtcNow);

        return new WatchRoomSnapshot(room, _stateStore.GetPlayback(roomCode), _stateStore.GetOnlineUserIds(roomCode));
    }

    public async Task LeaveRoom(string code)
    {
        var roomCode = NormalizeCode(code);
        if (JoinedRooms().Remove(roomCode))
            await LeaveRoomInternalAsync(roomCode);
    }

    public async Task SyncState(string code, string action, double positionSeconds)
    {
        var roomCode = EnsureJoined(code);
        if (!WatchRoomActions.IsValid(action))
            throw new HubException("Unknown action.");

        EnforceRateLimit("sync", MaxSyncCallsPerWindow);

        var state = _stateStore.ApplyAction(roomCode, action, positionSeconds);

        await Clients.OthersInGroup(GroupName(roomCode)).SendAsync("StateChanged", new
        {
            UserId = GetUserId(),
            Action = action,
            Playback = state
        });
    }

    public async Task Heartbeat(string code, double positionSeconds)
    {
        var roomCode = EnsureJoined(code);
        var userId = GetUserId();
        if (JoinedRooms()[roomCode] != userId)
            return;

        // A heartbeat sent just before someone paused must not start the room again.
        var current = _stateStore.GetPlayback(roomCode);
        if (!current.IsPlaying && DateTime.UtcNow - current.UpdatedAtUtc < RecentActionWindow)
            return;

        var state = _stateStore.ApplyAction(roomCode, WatchRoomActions.Play, positionSeconds);

        await Clients.OthersInGroup(GroupName(roomCode)).SendAsync("Heartbeat", new
        {
            UserId = userId,
            Playback = state
        });
    }

    public async Task SendMessage(string code, string content)
    {
        var roomCode = EnsureJoined(code);
        var text = content?.Trim();
        if (string.IsNullOrEmpty(text) || text.Length > MaxMessageLength)
            throw new HubException($"Message must be between 1 and {MaxMessageLength} characters.");

        EnforceRateLimit("message", MaxMessagesPerWindow);

        await Clients.Group(GroupName(roomCode)).SendAsync("ReceiveMessage", new
        {
            RoomCode = roomCode,
            SenderId = GetUserId(),
            SenderName = Context.Items[UserNameKey] as string,
            Content = text,
            SentAtUtc = DateTime.UtcNow
        });
    }

    public async Task SendReaction(string code, string emoji)
    {
        var roomCode = EnsureJoined(code);
        var value = emoji?.Trim();
        if (string.IsNullOrEmpty(value) || value.Length > MaxReactionLength)
            throw new HubException("Invalid reaction.");

        EnforceRateLimit("reaction", MaxReactionsPerWindow);

        await Clients.Group(GroupName(roomCode)).SendAsync("ReceiveReaction", new
        {
            RoomCode = roomCode,
            SenderId = GetUserId(),
            SenderName = Context.Items[UserNameKey] as string,
            Emoji = value,
            SentAtUtc = DateTime.UtcNow
        });
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        foreach (var roomCode in JoinedRooms().Keys.ToList())
            await LeaveRoomInternalAsync(roomCode);

        await base.OnDisconnectedAsync(exception);
    }

    private async Task LeaveRoomInternalAsync(string roomCode)
    {
        var userId = GetUserId();
        _stateStore.RemoveConnection(roomCode, Context.ConnectionId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(roomCode));

        if (!_stateStore.GetOnlineUserIds(roomCode).Contains(userId))
            await Clients.Group(GroupName(roomCode)).SendAsync("MemberLeft", new { UserId = userId });
    }

    private string EnsureJoined(string code)
    {
        var roomCode = NormalizeCode(code);
        // The store forgets a room's connections when it is closed, which ends everything this connection may do in it.
        if (!JoinedRooms().ContainsKey(roomCode) || !_stateStore.HasConnection(roomCode, Context.ConnectionId))
            throw new HubException("You have not joined this watch room.");
        return roomCode;
    }

    private Dictionary<string, string> JoinedRooms()
    {
        if (Context.Items[JoinedRoomsKey] is not Dictionary<string, string> rooms)
        {
            rooms = new Dictionary<string, string>();
            Context.Items[JoinedRoomsKey] = rooms;
        }
        return rooms;
    }

    private string GetUserId()
        => Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? throw new HubException("Authenticated user not found.");

    private static string NormalizeCode(string? code) => (code ?? string.Empty).Trim().ToUpperInvariant();

    public static string GroupName(string roomCode) => $"watchroom:{roomCode}";

    // Hub calls of one connection run one after another, so the timestamps need no locking.
    private void EnforceRateLimit(string bucket, int maxCalls)
    {
        var buckets = Context.Items[RateLimitKey] as Dictionary<string, Queue<DateTime>>;
        if (buckets is null)
        {
            buckets = new Dictionary<string, Queue<DateTime>>();
            Context.Items[RateLimitKey] = buckets;
        }

        if (!buckets.TryGetValue(bucket, out var calls))
            buckets[bucket] = calls = new Queue<DateTime>();

        var now = DateTime.UtcNow;
        while (calls.Count > 0 && now - calls.Peek() > RateLimitWindow)
            calls.Dequeue();

        if (calls.Count >= maxCalls)
            throw new HubException("You are doing that too fast. Slow down a little.");

        calls.Enqueue(now);
    }
}

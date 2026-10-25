using Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs;

public sealed class WatchRoomNotifier : IWatchRoomNotifier
{
    private readonly IHubContext<WatchRoomHub> _hubContext;
    private readonly IWatchRoomStateStore _stateStore;
    private readonly ILogger<WatchRoomNotifier> _logger;

    public WatchRoomNotifier(
        IHubContext<WatchRoomHub> hubContext,
        IWatchRoomStateStore stateStore,
        ILogger<WatchRoomNotifier> logger)
    {
        _hubContext = hubContext;
        _stateStore = stateStore;
        _logger = logger;
    }

    public async Task RoomClosedAsync(string roomCode, CancellationToken cancellationToken)
    {
        await _hubContext.Clients.Group(WatchRoomHub.GroupName(roomCode)).SendAsync("RoomClosed", new { RoomCode = roomCode }, cancellationToken);

        // From here on the hub treats every connection as no longer being in the room, so nothing more can be sent to it.
        _stateStore.RemoveRoom(roomCode);

        _logger.LogInformation("Watch room closed. Code: {Code}", roomCode);
    }
}

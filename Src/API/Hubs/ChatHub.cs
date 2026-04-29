using Application.Chats.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IMediator _mediator;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(IMediator mediator, ILogger<ChatHub> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
            _logger.LogInformation(
                "SignalR client connected. ConnectionId: {ConnectionId}, UserId: {UserId}, TimestampUtc: {TimestampUtc}",
                Context.ConnectionId,
                userId,
                DateTime.UtcNow);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation(
            "SignalR client disconnected. ConnectionId: {ConnectionId}, UserId: {UserId}, TimestampUtc: {TimestampUtc}",
            Context.ConnectionId,
            userId,
            DateTime.UtcNow);
        await base.OnDisconnectedAsync(exception);
    }

    public Task JoinChat(int chatId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, $"chat:{chatId}");
    }

    public Task LeaveChat(int chatId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat:{chatId}");
    }

    public async Task SendMessageToChat(int chatId, string content)
    {
        var senderId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _mediator.Send(new SendMessageCommand(new SendMessageRequest
        {
            ChatId = chatId,
            Content = content
        }));

        if (!result.Success || result.Data is null)
        {
            throw new HubException(result.Message ?? "Failed to send message.");
        }

        await Clients.Group($"chat:{chatId}").SendAsync("ReceiveMessage", result.Data);
        _logger.LogInformation(
            "SignalR message broadcasted. ChatId: {ChatId}, SenderId: {SenderId}, MessageId: {MessageId}, TimestampUtc: {TimestampUtc}",
            chatId,
            senderId,
            result.Data.Id,
            DateTime.UtcNow);
    }
}

using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Chats.Commands;

public sealed class MarkMessagesAsReadCommandHandler : IRequestHandler<MarkMessagesAsReadCommand, BaseResponse>
{
    private readonly IChatRepository _chatRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<MarkMessagesAsReadCommandHandler> _logger;

    public MarkMessagesAsReadCommandHandler(
        IChatRepository chatRepository,
        IMessageRepository messageRepository,
        ICurrentUserService currentUserService,
        ILogger<MarkMessagesAsReadCommandHandler> logger)
    {
        _chatRepository = chatRepository;
        _messageRepository = messageRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(MarkMessagesAsReadCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (string.IsNullOrWhiteSpace(currentUserId))
            return BaseResponse.Fail("Authenticated user not found.");

        var chat = await _chatRepository.GetByIdWithParticipantsAsync(request.ChatId, cancellationToken);
        if (chat is null)
            return BaseResponse.Fail("Chat not found.");

        if (!chat.Participants.Any(p => p.UserId == currentUserId))
            return BaseResponse.Fail("You are not a participant of this chat.");

        var updatedCount = await _messageRepository.MarkAsReadAsync(request.ChatId, currentUserId, cancellationToken);
        await _messageRepository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            "Messages marked as read. ChatId: {ChatId}, CurrentUserId: {CurrentUserId}, UpdatedCount: {UpdatedCount}, TimestampUtc: {TimestampUtc}",
            request.ChatId,
            currentUserId,
            updatedCount,
            DateTime.UtcNow);
        return BaseResponse.Ok("Messages marked as read.");
    }
}

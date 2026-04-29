using Application.Chats.Dtos;
using Application.Common.Interfaces;
using Application.Common.Responses;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Chats.Commands;

public sealed class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, BaseResponse<MessageDto>>
{
    private readonly IChatRepository _chatRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserReadService _userReadService;
    private readonly IMapper _mapper;
    private readonly ILogger<SendMessageCommandHandler> _logger;

    public SendMessageCommandHandler(
        IChatRepository chatRepository,
        IMessageRepository messageRepository,
        ICurrentUserService currentUserService,
        IUserReadService userReadService,
        IMapper mapper,
        ILogger<SendMessageCommandHandler> logger)
    {
        _chatRepository = chatRepository;
        _messageRepository = messageRepository;
        _currentUserService = currentUserService;
        _userReadService = userReadService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseResponse<MessageDto>> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (string.IsNullOrWhiteSpace(currentUserId))
            return BaseResponse<MessageDto>.Fail("Authenticated user not found.");

        if (request.Request.ChatId <= 0 || string.IsNullOrWhiteSpace(request.Request.Content))
            return BaseResponse<MessageDto>.Fail("ChatId and content are required.");

        var chat = await _chatRepository.GetByIdWithParticipantsAsync(request.Request.ChatId, cancellationToken);
        if (chat is null)
            return BaseResponse<MessageDto>.Fail("Chat not found.");

        if (!chat.Participants.Any(p => p.UserId == currentUserId))
            return BaseResponse<MessageDto>.Fail("You are not a participant of this chat.");

        var message = _mapper.Map<Message>(request);
        message.ChatId = chat.Id;
        message.SenderId = currentUserId;
        message.Content = request.Request.Content.Trim();
        message.CreatedAt = DateTime.UtcNow;
        message.IsRead = false;

        await _messageRepository.AddAsync(message, cancellationToken);
        await _messageRepository.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<MessageDto>(message);
        dto.SenderName = await _userReadService.GetUserNameAsync(message.SenderId, cancellationToken);
        _logger.LogInformation(
            "Message sent. ChatId: {ChatId}, SenderId: {SenderId}, MessageId: {MessageId}, TimestampUtc: {TimestampUtc}",
            message.ChatId,
            message.SenderId,
            message.Id,
            DateTime.UtcNow);
        return BaseResponse<MessageDto>.Ok(dto);
    }
}

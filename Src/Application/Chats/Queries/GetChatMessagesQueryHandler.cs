using Application.Chats.Dtos;
using Application.Common.Interfaces;
using Application.Common.Responses;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Chats.Queries;

public sealed class GetChatMessagesQueryHandler : IRequestHandler<GetChatMessagesQuery, BaseResponse<List<MessageDto>>>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IChatRepository _chatRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserReadService _userReadService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetChatMessagesQueryHandler> _logger;

    public GetChatMessagesQueryHandler(
        IMessageRepository messageRepository,
        IChatRepository chatRepository,
        ICurrentUserService currentUserService,
        IUserReadService userReadService,
        IMapper mapper,
        ILogger<GetChatMessagesQueryHandler> logger)
    {
        _messageRepository = messageRepository;
        _chatRepository = chatRepository;
        _currentUserService = currentUserService;
        _userReadService = userReadService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseResponse<List<MessageDto>>> Handle(GetChatMessagesQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (string.IsNullOrWhiteSpace(currentUserId))
            return BaseResponse<List<MessageDto>>.Fail("Authenticated user not found.");

        var chat = await _chatRepository.GetByIdWithParticipantsAsync(request.ChatId, cancellationToken);
        if (chat is null)
            return BaseResponse<List<MessageDto>>.Fail("Chat not found.");

        if (!chat.Participants.Any(p => p.UserId == currentUserId))
            return BaseResponse<List<MessageDto>>.Fail("You are not a participant of this chat.");

        var messages = await _messageRepository.GetMessagesByChatIdAsync(request.ChatId, cancellationToken);
        var data = _mapper.Map<List<MessageDto>>(messages);
        foreach (var message in data)
        {
            message.SenderName = await _userReadService.GetUserNameAsync(message.SenderId, cancellationToken);
        }
        _logger.LogInformation(
            "Chat messages fetched. ChatId: {ChatId}, CurrentUserId: {CurrentUserId}, MessageCount: {MessageCount}, TimestampUtc: {TimestampUtc}",
            request.ChatId,
            currentUserId,
            data.Count,
            DateTime.UtcNow);

        return BaseResponse<List<MessageDto>>.Ok(data);
    }
}

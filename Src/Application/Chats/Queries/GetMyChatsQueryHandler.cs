using Application.Chats.Dtos;
using Application.Common.Interfaces;
using Application.Common.Responses;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Chats.Queries;

public sealed class GetMyChatsQueryHandler : IRequestHandler<GetMyChatsQuery, BaseResponse<List<ChatDto>>>
{
    private readonly IChatRepository _chatRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserReadService _userReadService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetMyChatsQueryHandler> _logger;

    public GetMyChatsQueryHandler(
        IChatRepository chatRepository,
        ICurrentUserService currentUserService,
        IUserReadService userReadService,
        IMapper mapper,
        ILogger<GetMyChatsQueryHandler> logger)
    {
        _chatRepository = chatRepository;
        _currentUserService = currentUserService;
        _userReadService = userReadService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseResponse<List<ChatDto>>> Handle(GetMyChatsQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (string.IsNullOrWhiteSpace(currentUserId))
            return BaseResponse<List<ChatDto>>.Fail("Authenticated user not found.");

        var chats = await _chatRepository.GetChatsByUserIdAsync(currentUserId, cancellationToken);
        var data = _mapper.Map<List<ChatDto>>(chats);
        for (var i = 0; i < data.Count; i++)
        {
            var chat = data[i];
            var sourceChat = chats[i];
            chat.UnreadCount = sourceChat.Messages.Count(m => !m.IsRead && m.SenderId != currentUserId);

            foreach (var participant in chat.Participants)
            {
                participant.UserName = await _userReadService.GetUserNameAsync(participant.UserId, cancellationToken);
            }
            if (chat.LastMessage is not null)
            {
                chat.LastMessage.SenderName = await _userReadService.GetUserNameAsync(chat.LastMessage.SenderId, cancellationToken);
            }
        }
        _logger.LogInformation(
            "My chats fetched. CurrentUserId: {CurrentUserId}, ChatCount: {ChatCount}, TimestampUtc: {TimestampUtc}",
            currentUserId,
            data.Count,
            DateTime.UtcNow);
        return BaseResponse<List<ChatDto>>.Ok(data);
    }
}

using Application.Chats.Dtos;
using Application.Common.Interfaces;
using Application.Common.Responses;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Chats.Commands;

public sealed class CreatePrivateChatCommandHandler : IRequestHandler<CreatePrivateChatCommand, BaseResponse<ChatDto>>
{
    private readonly IChatRepository _chatRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserReadService _userReadService;
    private readonly IMapper _mapper;
    private readonly ILogger<CreatePrivateChatCommandHandler> _logger;

    public CreatePrivateChatCommandHandler(
        IChatRepository chatRepository,
        ICurrentUserService currentUserService,
        IUserReadService userReadService,
        IMapper mapper,
        ILogger<CreatePrivateChatCommandHandler> logger)
    {
        _chatRepository = chatRepository;
        _currentUserService = currentUserService;
        _userReadService = userReadService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseResponse<ChatDto>> Handle(CreatePrivateChatCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (string.IsNullOrWhiteSpace(currentUserId))
            return BaseResponse<ChatDto>.Fail("Authenticated user not found.");

        if (string.IsNullOrWhiteSpace(request.ReceiverUserId))
            return BaseResponse<ChatDto>.Fail("Receiver user is required.");

        if (string.Equals(currentUserId, request.ReceiverUserId, StringComparison.OrdinalIgnoreCase))
            return BaseResponse<ChatDto>.Fail("You cannot start chat with yourself.");

        var existing = await _chatRepository.GetPrivateChatByUsersAsync(currentUserId, request.ReceiverUserId, cancellationToken);
        if (existing is not null)
        {
            var existingDto = await EnrichUserNamesAsync(_mapper.Map<ChatDto>(existing), cancellationToken);
            _logger.LogInformation(
                "Existing private chat returned. ChatId: {ChatId}, CurrentUserId: {CurrentUserId}, ReceiverUserId: {ReceiverUserId}, TimestampUtc: {TimestampUtc}",
                existing.Id,
                currentUserId,
                request.ReceiverUserId,
                DateTime.UtcNow);
            return BaseResponse<ChatDto>.Ok(existingDto, "Existing private chat returned.");
        }

        var chat = _mapper.Map<Chat>(request);
        chat.CreatedAt = DateTime.UtcNow;
        chat.Participants =
        [
            new ChatParticipant { UserId = currentUserId, JoinedAt = DateTime.UtcNow },
            new ChatParticipant { UserId = request.ReceiverUserId, JoinedAt = DateTime.UtcNow }
        ];

        await _chatRepository.AddAsync(chat, cancellationToken);
        await _chatRepository.SaveChangesAsync(cancellationToken);

        var created = await _chatRepository.GetByIdWithParticipantsAsync(chat.Id, cancellationToken) ?? chat;
        var createdDto = await EnrichUserNamesAsync(_mapper.Map<ChatDto>(created), cancellationToken);
        _logger.LogInformation(
            "Private chat created. ChatId: {ChatId}, CurrentUserId: {CurrentUserId}, ReceiverUserId: {ReceiverUserId}, TimestampUtc: {TimestampUtc}",
            created.Id,
            currentUserId,
            request.ReceiverUserId,
            DateTime.UtcNow);
        return BaseResponse<ChatDto>.Ok(createdDto, "Private chat created successfully.");
    }

    private async Task<ChatDto> EnrichUserNamesAsync(ChatDto dto, CancellationToken cancellationToken)
    {
        foreach (var participant in dto.Participants)
        {
            participant.UserName = await _userReadService.GetUserNameAsync(participant.UserId, cancellationToken);
        }

        if (dto.LastMessage is not null)
        {
            dto.LastMessage.SenderName = await _userReadService.GetUserNameAsync(dto.LastMessage.SenderId, cancellationToken);
        }

        return dto;
    }
}

using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.WatchRooms.Dtos;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.WatchRooms.Commands;

public class JoinWatchRoomCommandHandler : IRequestHandler<JoinWatchRoomCommand, BaseResponse<GetWatchRoomByIdResponse>>
{
    private readonly IWatchRoomRepository _watchRoomRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserReadService _userReadService;
    private readonly IMapper _mapper;
    private readonly ILogger<JoinWatchRoomCommandHandler> _logger;

    public JoinWatchRoomCommandHandler(
        IWatchRoomRepository watchRoomRepository,
        ICurrentUserService currentUserService,
        IUserReadService userReadService,
        IMapper mapper,
        ILogger<JoinWatchRoomCommandHandler> logger)
    {
        _watchRoomRepository = watchRoomRepository;
        _currentUserService = currentUserService;
        _userReadService = userReadService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseResponse<GetWatchRoomByIdResponse>> Handle(JoinWatchRoomCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        _logger.LogInformation(
            "JoinWatchRoomCommand started. Code: {Code}, UserId: {UserId}",
            request.Code,
            currentUserId);

        if (string.IsNullOrWhiteSpace(currentUserId))
            return BaseResponse<GetWatchRoomByIdResponse>.Fail("Authenticated user not found.");

        var room = await _watchRoomRepository.GetByCodeAsync(request.Code.Trim().ToUpperInvariant(), cancellationToken);

        if (room is null || !room.IsActive)
        {
            _logger.LogWarning(
                "JoinWatchRoomCommand failed. Watch room not found. Code: {Code}",
                request.Code);

            return BaseResponse<GetWatchRoomByIdResponse>.Fail("Watch room not found.");
        }

        if (room.Members.All(m => m.UserId != currentUserId))
        {
            room.Members.Add(new WatchRoomMember { UserId = currentUserId, JoinedAt = DateTime.UtcNow });
            await _watchRoomRepository.SaveChangesAsync(cancellationToken);
        }

        var response = _mapper.Map<GetWatchRoomByIdResponse>(room);
        foreach (var member in response.Members)
            member.UserName = await _userReadService.GetUserNameAsync(member.UserId, cancellationToken);

        _logger.LogInformation(
            "JoinWatchRoomCommand completed successfully. WatchRoomId: {WatchRoomId}, UserId: {UserId}",
            room.Id,
            currentUserId);

        return BaseResponse<GetWatchRoomByIdResponse>.Ok(response, "Joined watch room successfully.");
    }
}

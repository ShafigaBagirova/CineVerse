using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.WatchRooms.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.WatchRooms.Queries;

public class GetWatchRoomByIdQueryHandler
    : IRequestHandler<GetWatchRoomByIdQuery, BaseResponse<GetWatchRoomByIdResponse>>
{
    private readonly IWatchRoomRepository _watchRoomRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserReadService _userReadService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetWatchRoomByIdQueryHandler> _logger;

    public GetWatchRoomByIdQueryHandler(
        IWatchRoomRepository watchRoomRepository,
        ICurrentUserService currentUserService,
        IUserReadService userReadService,
        IMapper mapper,
        ILogger<GetWatchRoomByIdQueryHandler> logger)
    {
        _watchRoomRepository = watchRoomRepository;
        _currentUserService = currentUserService;
        _userReadService = userReadService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseResponse<GetWatchRoomByIdResponse>> Handle(
        GetWatchRoomByIdQuery request,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        _logger.LogInformation(
            "GetWatchRoomByIdQuery started. WatchRoomId: {WatchRoomId}, UserId: {UserId}",
            request.Id,
            currentUserId);

        if (string.IsNullOrWhiteSpace(currentUserId))
            return BaseResponse<GetWatchRoomByIdResponse>.Fail("Authenticated user not found.");

        var room = await _watchRoomRepository.GetByIdWithMembersAsync(request.Id, cancellationToken);

        if (room is null || !room.IsActive || room.Members.All(m => m.UserId != currentUserId))
        {
            _logger.LogWarning(
                "GetWatchRoomByIdQuery failed. Watch room not found or not accessible. WatchRoomId: {WatchRoomId}",
                request.Id);

            return BaseResponse<GetWatchRoomByIdResponse>.Fail("Watch room not found.");
        }

        var response = _mapper.Map<GetWatchRoomByIdResponse>(room);
        foreach (var member in response.Members)
            member.UserName = await _userReadService.GetUserNameAsync(member.UserId, cancellationToken);

        _logger.LogInformation(
            "GetWatchRoomByIdQuery completed successfully. WatchRoomId: {WatchRoomId}",
            request.Id);

        return BaseResponse<GetWatchRoomByIdResponse>.Ok(response, "Watch room fetched successfully.");
    }
}

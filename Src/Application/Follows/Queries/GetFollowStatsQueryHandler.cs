using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Follows.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Follows.Queries;

public sealed class GetFollowStatsQueryHandler
    : IRequestHandler<GetFollowStatsQuery, BaseResponse<FollowStatsDto>>
{
    private readonly IFollowRepository _followRepository;
    private readonly IIdentityService _identityService;
    private readonly ILogger<GetFollowStatsQueryHandler> _logger;

    public GetFollowStatsQueryHandler(
        IFollowRepository followRepository,
        IIdentityService identityService,
        ILogger<GetFollowStatsQueryHandler> logger)
    {
        _followRepository = followRepository;
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<BaseResponse<FollowStatsDto>> Handle(GetFollowStatsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetFollowStatsQuery started. Target user id: {UserId}", request.UserId);

        var userExists = await _identityService.UserExistsAsync(request.UserId);
        if (!userExists)
        {
            _logger.LogWarning(
                "GetFollowStatsQuery failed. Target user not found. UserId: {UserId}",
                request.UserId);

            return BaseResponse<FollowStatsDto>.Fail(FollowMessages.UserNotFound);
        }

        var followersCount = await _followRepository.GetFollowersCountAsync(request.UserId, cancellationToken);
        var followingsCount = await _followRepository.GetFollowingsCountAsync(request.UserId, cancellationToken);

        var dto = new FollowStatsDto
        {
            FollowersCount = followersCount,
            FollowingsCount = followingsCount
        };

        _logger.LogInformation(
            "GetFollowStatsQuery completed successfully. UserId: {UserId}, FollowersCount: {FollowersCount}, FollowingsCount: {FollowingsCount}",
            request.UserId,
            followersCount,
            followingsCount);

        return BaseResponse<FollowStatsDto>.Ok(dto,FollowMessages.FollowStatsRetrieved);
    }
}

using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Follows.Commands;

public sealed class UnfollowUserCommandHandler
    : IRequestHandler<UnfollowUserCommand, BaseResponse>
{
    private readonly IFollowRepository _followRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdentityService _identityService;
    private readonly ILogger<UnfollowUserCommandHandler> _logger;
    private readonly ICacheService _cacheService;

    public UnfollowUserCommandHandler(
        IFollowRepository followRepository,
        ICurrentUserService currentUserService,
        IIdentityService identityService,
        ILogger<UnfollowUserCommandHandler> logger,
        ICacheService cacheService)
    {
        _followRepository = followRepository;
        _currentUserService = currentUserService;
        _identityService = identityService;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse> Handle(UnfollowUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("UnfollowUserCommand started. Target user id: {FollowingId}", request.FollowingId);

        var followerId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(followerId))
        {
            _logger.LogWarning("UnfollowUserCommand failed. Current user is not authenticated.");
            return BaseResponse.Fail(FollowMessages.NotAuthenticated);
        }

        if (string.IsNullOrWhiteSpace(request.FollowingId))
        {
            _logger.LogWarning("UnfollowUserCommand failed. FollowingId is empty. Current user: {FollowerId}", followerId);
            return BaseResponse.Fail(FollowMessages.FollowingIdRequired);
        }

        if (followerId == request.FollowingId)
        {
            _logger.LogWarning("UnfollowUserCommand failed. User tried to unfollow themselves. UserId: {FollowerId}", followerId);
            return BaseResponse.Fail(FollowMessages.CannotFollowSelf);
        }

        var userExists = await _identityService.UserExistsAsync(request.FollowingId);
        if (!userExists)
        {
            _logger.LogWarning(
                "UnfollowUserCommand failed. Target user not found. Current user: {FollowerId}, Target user: {FollowingId}",
                followerId,
                request.FollowingId);

            return BaseResponse.Fail(FollowMessages.UserNotFound);
        }

        var follow = await _followRepository.GetAsync(followerId, request.FollowingId, cancellationToken);
        if (follow is null)
        {
            _logger.LogWarning(
                "UnfollowUserCommand failed. Follow relationship not found. Current user: {FollowerId}, Target user: {FollowingId}",
                followerId,
                request.FollowingId);

            return BaseResponse.Fail(FollowMessages.NotFollowing);
        }

        await _followRepository.DeleteAsync(follow, cancellationToken);
        await _cacheService.RemoveAsync(FollowCacheKeys.FollowersPattern(request.FollowingId),
         cancellationToken);

        await _cacheService.RemoveAsync( FollowCacheKeys.FollowingsPattern(followerId),
        cancellationToken);

        _logger.LogInformation(
            "UnfollowUserCommand completed successfully. Current user: {FollowerId}, Target user: {FollowingId}",
            followerId,
            request.FollowingId);

        return BaseResponse.Ok(FollowMessages.UnfollowSuccess);
    }
}
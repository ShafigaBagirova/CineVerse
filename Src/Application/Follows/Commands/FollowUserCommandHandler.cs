using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Follows.Commands;

public sealed class FollowUserCommandHandler
    : IRequestHandler<FollowUserCommand, BaseResponse>
{
    private readonly IFollowRepository _followRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdentityService _identityService;
    private readonly ILogger<FollowUserCommandHandler> _logger;
    private readonly ICacheService _cacheService;

    public FollowUserCommandHandler(
        IFollowRepository followRepository,
        ICurrentUserService currentUserService,
        IIdentityService identityService,
        ILogger<FollowUserCommandHandler> logger,
        ICacheService cacheService)
    {
        _followRepository = followRepository;
        _currentUserService = currentUserService;
        _identityService = identityService;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse> Handle(FollowUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("FollowUserCommand started. Target user id: {FollowingId}", request.FollowingId);

        var followerId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(followerId))
        {
            _logger.LogWarning("FollowUserCommand failed. Current user is unauthorized.");
            return BaseResponse.Fail(FollowMessages.NotAuthenticated);
        }

        if (string.IsNullOrWhiteSpace(request.FollowingId))
        {
            _logger.LogWarning("FollowUserCommand failed. FollowingId is empty. Current user: {FollowerId}", followerId);
            return BaseResponse.Fail(FollowMessages.FollowingIdRequired);
        }

        if (followerId == request.FollowingId)
        {
            _logger.LogWarning("FollowUserCommand failed. User tried to follow themselves. UserId: {FollowerId}", followerId);
            return BaseResponse.Fail(FollowMessages.CannotFollowSelf);
        }

        var userExists = await _identityService.UserExistsAsync(request.FollowingId);
        if (!userExists)
        {
            _logger.LogWarning(
                "FollowUserCommand failed. Target user not found. Current user: {FollowerId}, Target user: {FollowingId}",
                followerId,
                request.FollowingId);

            return BaseResponse.Fail(FollowMessages.UserNotFound);
        }

        var alreadyExists = await _followRepository.ExistsAsync(followerId, request.FollowingId, cancellationToken);
        if (alreadyExists)
        {
            _logger.LogWarning(
                "FollowUserCommand failed. Follow relationship already exists. Current user: {FollowerId}, Target user: {FollowingId}",
                followerId,
                request.FollowingId);

            return BaseResponse.Fail(FollowMessages.AlreadyFollowing);
        }

        var follow = new Follow
        {
            FollowerId = followerId,
            FollowingId = request.FollowingId,
            CreatedAt = DateTime.UtcNow
        };

        await _followRepository.AddAsync(follow, cancellationToken);
        await _followRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync(FollowCacheKey.FollowersPattern(request.FollowingId),
        cancellationToken);

        await _cacheService.RemoveAsync(FollowCacheKey.FollowingsPattern(followerId),
            cancellationToken);

        _logger.LogInformation(
            "FollowUserCommand completed successfully. Current user: {FollowerId}, Target user: {FollowingId}",
            followerId,
            request.FollowingId);

        return BaseResponse.Ok(FollowMessages.FollowSuccess);
    }
}

using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Follows.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Follows.Queries;

public sealed class GetFollowStatusQueryHandler
    : IRequestHandler<GetFollowStatusQuery, BaseResponse<FollowStatusDto>>
{
    private readonly IFollowRepository _followRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdentityService _identityService;
    private readonly ILogger<GetFollowStatusQueryHandler> _logger;

    public GetFollowStatusQueryHandler(
        IFollowRepository followRepository,
        ICurrentUserService currentUserService,
        IIdentityService identityService,
        ILogger<GetFollowStatusQueryHandler> logger)
    {
        _followRepository = followRepository;
        _currentUserService = currentUserService;
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<BaseResponse<FollowStatusDto>> Handle(GetFollowStatusQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetFollowStatusQuery started. Target user: {UserId}", request.UserId);

        var currentUserId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            _logger.LogWarning("GetFollowStatusQuery failed. User not authenticated.");
            return BaseResponse<FollowStatusDto>.Fail("User is not authenticated.");
        }

        var userExists = await _identityService.UserExistsAsync(request.UserId);
        if (!userExists)
        {
            _logger.LogWarning("GetFollowStatusQuery failed. Target user not found. UserId: {UserId}", request.UserId);
            return BaseResponse<FollowStatusDto>.Fail("User not found.");
        }

        var isFollowing = await _followRepository
            .ExistsAsync(currentUserId, request.UserId, cancellationToken);

        var dto = new FollowStatusDto
        {
            IsFollowing = isFollowing
        };

        _logger.LogInformation(
            "GetFollowStatusQuery completed. Current user: {CurrentUserId}, Target user: {UserId}, IsFollowing: {IsFollowing}",
            currentUserId,
            request.UserId,
            isFollowing);

        return BaseResponse<FollowStatusDto>.Ok(dto);
    }
}
using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Follows.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Follows.Queries;

public sealed class GetFollowRelationshipQueryHandler
    : IRequestHandler<GetFollowRelationshipQuery, BaseResponse<FollowRelationshipDto>>
{
    private readonly IFollowRepository _followRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdentityService _identityService;
    private readonly ILogger<GetFollowRelationshipQueryHandler> _logger;

    public GetFollowRelationshipQueryHandler(
        IFollowRepository followRepository,
        ICurrentUserService currentUserService,
        IIdentityService identityService,
        ILogger<GetFollowRelationshipQueryHandler> logger)
    {
        _followRepository = followRepository;
        _currentUserService = currentUserService;
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<BaseResponse<FollowRelationshipDto>> Handle(
        GetFollowRelationshipQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "GetFollowRelationshipQuery started. Target user id: {UserId}",
            request.UserId);

        var currentUserId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            _logger.LogWarning("GetFollowRelationshipQuery failed. User is not authenticated.");
            return BaseResponse<FollowRelationshipDto>.Fail(FollowMessages.NotAuthenticated);
        }

        var userExists = await _identityService.UserExistsAsync(request.UserId);
        if (!userExists)
        {
            _logger.LogWarning(
                "GetFollowRelationshipQuery failed. Target user not found. UserId: {UserId}",
                request.UserId);

            return BaseResponse<FollowRelationshipDto>.Fail(FollowMessages.UserNotFound);
        }

        var isSelf = currentUserId == request.UserId;

        var isFollowing = false;
        var isFollowedBy = false;

        if (!isSelf)
        {
            isFollowing = await _followRepository.ExistsAsync(
                currentUserId,
                request.UserId,
                cancellationToken);

            isFollowedBy = await _followRepository.ExistsAsync(
                request.UserId,
                currentUserId,
                cancellationToken);
        }

        var dto = new FollowRelationshipDto
        {
            IsFollowing = isFollowing,
            IsFollowedBy = isFollowedBy,
            IsMutual = isFollowing && isFollowedBy,
            IsSelf = isSelf
        };

        _logger.LogInformation(
            "GetFollowRelationshipQuery completed successfully. CurrentUserId: {CurrentUserId}, TargetUserId: {TargetUserId}, IsFollowing: {IsFollowing}, IsFollowedBy: {IsFollowedBy}, IsMutual: {IsMutual}, IsSelf: {IsSelf}",
            currentUserId,
            request.UserId,
            dto.IsFollowing,
            dto.IsFollowedBy,
            dto.IsMutual,
            dto.IsSelf);

        return BaseResponse<FollowRelationshipDto>.Ok(
            dto,
            FollowMessages.FollowRelationshipRetrieved);
    }
}
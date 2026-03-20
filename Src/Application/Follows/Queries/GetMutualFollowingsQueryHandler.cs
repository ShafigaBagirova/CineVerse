using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Follows.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Follows.Queries;

public sealed class GetMutualFollowingsQueryHandler
    : IRequestHandler<GetMutualFollowingsQuery, BaseResponse<PaginatedResponse<FollowUserItemDto>>>
{
    private readonly IFollowRepository _followRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdentityService _identityService;
    private readonly ILogger<GetMutualFollowingsQueryHandler> _logger;

    public GetMutualFollowingsQueryHandler(
        IFollowRepository followRepository,
        ICurrentUserService currentUserService,
        IIdentityService identityService,
        ILogger<GetMutualFollowingsQueryHandler> logger)
    {
        _followRepository = followRepository;
        _currentUserService = currentUserService;
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<BaseResponse<PaginatedResponse<FollowUserItemDto>>> Handle(
        GetMutualFollowingsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "GetMutualFollowingsQuery started. TargetUserId: {UserId}, Page: {Page}, PageSize: {PageSize}",
            request.UserId,
            request.Page,
            request.PageSize);

        var currentUserId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            _logger.LogWarning("GetMutualFollowingsQuery failed. User is not authenticated.");
            return BaseResponse<PaginatedResponse<FollowUserItemDto>>.Fail(FollowMessages.NotAuthenticated);
        }

        var userExists = await _identityService.UserExistsAsync(request.UserId);
        if (!userExists)
        {
            _logger.LogWarning(
                "GetMutualFollowingsQuery failed. Target user not found. UserId: {UserId}",
                request.UserId);

            return BaseResponse<PaginatedResponse<FollowUserItemDto>>.Fail(FollowMessages.UserNotFound);
        }

        var totalCount = await _followRepository.GetMutualFollowingsTotalCountAsync(
            currentUserId,
            request.UserId,
            cancellationToken);

        var mutualIds = await _followRepository.GetMutualFollowingIdsAsync(
            currentUserId,
            request.UserId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var users = await _identityService.GetUsersByIdsAsync(mutualIds, cancellationToken);

        var orderedUsers = mutualIds
            .Select(id => users.FirstOrDefault(x => x.UserId == id))
            .Where(x => x is not null)
            .Cast<FollowUserItemDto>()
            .ToList();

        var paginated = new PaginatedResponse<FollowUserItemDto>
        {
            Items = orderedUsers,
            TotalCount = totalCount,
            PageNumber = request.Page,
            PageSize = request.PageSize
        };

        _logger.LogInformation(
            "GetMutualFollowingsQuery completed successfully. CurrentUserId: {CurrentUserId}, TargetUserId: {TargetUserId}, TotalCount: {TotalCount}",
            currentUserId,
            request.UserId,
            totalCount);

        return BaseResponse<PaginatedResponse<FollowUserItemDto>>.Ok(
            paginated,
            FollowMessages.MutualFollowingsRetrieved);
    }
}
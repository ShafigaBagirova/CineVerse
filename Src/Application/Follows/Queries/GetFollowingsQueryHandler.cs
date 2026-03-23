using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Follows.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Follows.Queries;

public sealed class GetFollowingsQueryHandler
    : IRequestHandler<GetFollowingsQuery, BaseResponse<PaginatedResponse<FollowUserItemDto>>>
{
    private readonly IFollowRepository _followRepository;
    private readonly IIdentityService _identityService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetFollowingsQueryHandler> _logger;

    public GetFollowingsQueryHandler(
        IFollowRepository followRepository,
        IIdentityService identityService,
        ICacheService cacheService,
        ILogger<GetFollowingsQueryHandler> logger)
    {
        _followRepository = followRepository;
        _identityService = identityService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<BaseResponse<PaginatedResponse<FollowUserItemDto>>> Handle(
        GetFollowingsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "GetFollowingsQuery started. UserId: {UserId}, Page: {Page}, PageSize: {PageSize}",
            request.UserId,
            request.Page,
            request.PageSize);

        var userExists = await _identityService.UserExistsAsync(request.UserId);
        if (!userExists)
        {
            _logger.LogWarning(
                "GetFollowingsQuery failed. User not found. UserId: {UserId}",
                request.UserId);

            return BaseResponse<PaginatedResponse<FollowUserItemDto>>.Fail(FollowMessages.UserNotFound);
        }

        var cacheKey = FollowCacheKey.Followings(request.UserId, request.Page, request.PageSize);

        var cached = await _cacheService.GetAsync<PaginatedResponse<FollowUserItemDto>>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            _logger.LogInformation(
                "GetFollowingsQuery cache hit. UserId: {UserId}, Page: {Page}, PageSize: {PageSize}",
                request.UserId,
                request.Page,
                request.PageSize);

            return BaseResponse<PaginatedResponse<FollowUserItemDto>>.Ok(cached, FollowMessages.FollowingsRetrieved);
        }

        var totalCount = await _followRepository.GetFollowingsCountAsync(
            request.UserId,
            cancellationToken);

        var followingIds = await _followRepository.GetFollowingIdsAsync(
            request.UserId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var users = await _identityService.GetUsersByIdsAsync(followingIds, cancellationToken);

        var orderedUsers = followingIds
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

        await _cacheService.SetAsync(
            cacheKey,
            paginated,
            TimeSpan.FromMinutes(5),
            cancellationToken);

        _logger.LogInformation(
            "GetFollowingsQuery completed successfully. UserId: {UserId}, TotalCount: {TotalCount}, ReturnedCount: {ReturnedCount}",
            request.UserId,
            totalCount,
            orderedUsers.Count);

        return BaseResponse<PaginatedResponse<FollowUserItemDto>>.Ok(
            paginated,
            FollowMessages.FollowingsRetrieved);
    }
}
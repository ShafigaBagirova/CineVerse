using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Follows.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Follows.Queries;

public sealed class GetFollowInsightsQueryHandler
    : IRequestHandler<GetFollowInsightsQuery, BaseResponse<FollowInsightsDto>>
{
    private readonly IFollowRepository _followRepository;
    private readonly IUserSuggestionRepository _suggestionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdentityService _identityService;
    private readonly ILogger<GetFollowInsightsQueryHandler> _logger;

    public GetFollowInsightsQueryHandler(
        IFollowRepository followRepository,
        IUserSuggestionRepository suggestionRepository,
        ICurrentUserService currentUserService,
        IIdentityService identityService,
        ILogger<GetFollowInsightsQueryHandler> logger)
    {
        _followRepository = followRepository;
        _suggestionRepository = suggestionRepository;
        _currentUserService = currentUserService;
        _identityService = identityService;
        _logger = logger;
    }

    public async Task<BaseResponse<FollowInsightsDto>> Handle(
        GetFollowInsightsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetFollowInsightsQuery started. UserId: {UserId}", request.UserId);

        var currentUserId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(currentUserId))
            return BaseResponse<FollowInsightsDto>.Fail("User is not authenticated.");

        var userExists = await _identityService.UserExistsAsync(request.UserId);
        if (!userExists)
            return BaseResponse<FollowInsightsDto>.Fail(FollowMessages.UserNotFound);

        var followersCount = await _followRepository.GetFollowersCountAsync(request.UserId, cancellationToken);
        var followingsCount = await _followRepository.GetFollowingsCountAsync(request.UserId, cancellationToken);

        var mutualFollowings = await _followRepository.GetMutualFollowingsTotalCountAsync(
            currentUserId,
            request.UserId,
            cancellationToken);

        var suggestedCount = await _suggestionRepository.GetSuggestedUsersByTasteCountAsync(
            request.UserId,
            cancellationToken);

        double similarityScore = 0;

        if (mutualFollowings > 0)
        {
            similarityScore = Math.Min(100, mutualFollowings * 5);
        }

        var dto = new FollowInsightsDto
        {
            FollowersCount = followersCount,
            FollowingsCount = followingsCount,
            MutualFollowersCount = 0,
            MutualFollowingsCount = mutualFollowings,
            SuggestedUsersCount = suggestedCount,
            TasteSimilarityScore = similarityScore
        };

        _logger.LogInformation(
            "GetFollowInsightsQuery completed. UserId: {UserId}",
            request.UserId);

        return BaseResponse<FollowInsightsDto>.Ok(dto, FollowMessages.FollowInsightsRetrieved);
    }
}
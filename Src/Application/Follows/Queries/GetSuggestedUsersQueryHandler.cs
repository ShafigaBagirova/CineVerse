using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Follows.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Follows.Queries;

public sealed class GetSuggestedUsersQueryHandler
    : IRequestHandler<GetSuggestedUsersQuery, BaseResponse<PaginatedResponse<SuggestedUserItemDto>>>
{
    private readonly IRecommendationService _recommendationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetSuggestedUsersQueryHandler> _logger;

    public GetSuggestedUsersQueryHandler(
        IRecommendationService recommendationService,
        ICurrentUserService currentUserService,
        ILogger<GetSuggestedUsersQueryHandler> logger)
    {
        _recommendationService = recommendationService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<BaseResponse<PaginatedResponse<SuggestedUserItemDto>>> Handle(
        GetSuggestedUsersQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "GetSuggestedUsersQuery started. Page: {Page}, PageSize: {PageSize}",
            request.Page,
            request.PageSize);

        var currentUserId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(currentUserId))
            return BaseResponse<PaginatedResponse<SuggestedUserItemDto>>.Fail("User is not authenticated.");

        var paginated = await _recommendationService.GetSuggestedUsersAsync(
            currentUserId,
            request.Page,
            request.PageSize,
            cancellationToken);

        _logger.LogInformation(
            "GetSuggestedUsersQuery completed successfully. CurrentUserId: {CurrentUserId}, ReturnedCount: {ReturnedCount}",
            currentUserId,
            paginated.Items.Count);

        return BaseResponse<PaginatedResponse<SuggestedUserItemDto>>.Ok(
            paginated,
            "Suggested users retrieved successfully.");
    }
}
using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Movies.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Movies.Queries;

public sealed class GetSuggestedMoviesQueryHandler
    : IRequestHandler<GetSuggestedMoviesQuery, BaseResponse<PaginatedResponse<GetSuggestedMoviesResponse>>>
{
    private readonly IRecommendationService _recommendationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetSuggestedMoviesQueryHandler> _logger;

    public GetSuggestedMoviesQueryHandler(
        IRecommendationService recommendationService,
        ICurrentUserService currentUserService,
        ILogger<GetSuggestedMoviesQueryHandler> logger)
    {
        _recommendationService = recommendationService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<BaseResponse<PaginatedResponse<GetSuggestedMoviesResponse>>> Handle(
        GetSuggestedMoviesQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("GetSuggestedMoviesQuery failed. Authenticated user not found.");
            return BaseResponse<PaginatedResponse<GetSuggestedMoviesResponse>>.Fail("Authenticated user not found.");
        }

        _logger.LogInformation(
            "GetSuggestedMoviesQuery started. UserId: {UserId}, PageNumber: {PageNumber}, PageSize: {PageSize}",
            userId,
            request.Request.PageNumber,
            request.Request.PageSize);

        var response = await _recommendationService.GetSuggestedMoviesAsync(
            userId,
            request.Request.PageNumber,
            request.Request.PageSize,
            cancellationToken);

        _logger.LogInformation(
            "GetSuggestedMoviesQuery completed successfully. UserId: {UserId}, TotalCount: {TotalCount}",
            userId,
            response.TotalCount);

        return BaseResponse<PaginatedResponse<GetSuggestedMoviesResponse>>.Ok(
            response,
            "Suggested movies retrieved successfully.");
    }
}
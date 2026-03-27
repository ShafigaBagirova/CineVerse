using Application.Common.Responses;
using Application.Follows.Dtos;
using Application.Movies.Dtos;
using Application.Movies.Queries;
using FluentValidation;

namespace Application.Common.Interfaces;

public interface IRecommendationService
{
    Task<PaginatedResponse<GetSuggestedMoviesResponse>> GetSuggestedMoviesAsync(
        string userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
    Task<PaginatedResponse<SuggestedUserItemDto>> GetSuggestedUsersAsync(
    string userId,
    int pageNumber,
    int pageSize,
    CancellationToken cancellationToken);
}
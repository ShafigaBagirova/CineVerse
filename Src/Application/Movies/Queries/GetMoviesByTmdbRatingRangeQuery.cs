using Application.Common.Responses;
using Application.Movies.Dtos;
using MediatR;

namespace Application.Movies.Queries;

public sealed record GetMoviesByTmdbRatingRangeQuery(
    decimal MinRating,
    decimal MaxRating,
    int PageNumber = 1,
    int PageSize = 10)
    : IRequest<PaginatedResponse<GetAllMoviesResponse>>;

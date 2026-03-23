using Application.Common.Responses;
using Application.Movies.Dtos;
using Domain.Enums;
using MediatR;

namespace Application.Movies.Queries;

public record GetAllMoviesQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? Search = null,
    int? GenreId = null,
    string? Language = null,
    MovieStatus? Status=null,
    int? Year = null,
    decimal? MinTmdbRating = null,
    decimal? MaxTmdbRating = null,
    decimal? MinUserRating = null,
    decimal? MaxUserRating = null,
    string? SortBy = null,
    bool Desc = false
) : IRequest<BaseResponse<PaginatedResponse<GetAllMoviesResponse>>>;
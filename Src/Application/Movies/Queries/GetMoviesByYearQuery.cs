using Application.Common.Responses;
using Application.Movies.Dtos;
using MediatR;

namespace Application.Movies.Queries;

public sealed record GetMoviesByYearQuery(
    int Year,
    int PageNumber = 1,
    int PageSize = 10)
    : IRequest<PaginatedResponse<GetAllMoviesResponse>>;

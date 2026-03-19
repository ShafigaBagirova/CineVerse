using Application.Common.Responses;
using Application.Movies.Dtos;
using MediatR;

namespace Application.Movies.Queries;

public sealed record GetMoviesByGenreQuery(
    int GenreId,
    int Page,
    int PageSize)
    : IRequest<PaginatedResponse<GetAllMoviesResponse>>;
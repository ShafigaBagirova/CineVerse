using Application.Common.Responses;
using Application.Movies.Dtos;
using Domain.Enums;
using MediatR;

namespace Application.Movies.Queries;

public sealed record GetAllMoviesQuery(
    int PageNumber = 1,
    int PageSize = 10,
    MovieSortBy SortBy = MovieSortBy.ReleaseDate)
    : IRequest<PaginatedResponse<GetAllMoviesResponse>>;
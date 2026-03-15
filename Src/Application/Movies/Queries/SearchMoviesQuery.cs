using Application.Common.Responses;
using Application.Movies.Dtos;
using MediatR;

namespace Application.Movies.Queries;

public record SearchMoviesQuery(string SearchTerm, int PageNumber, int PageSize)
    : IRequest<PaginatedResponse<GetAllMoviesResponse>>;

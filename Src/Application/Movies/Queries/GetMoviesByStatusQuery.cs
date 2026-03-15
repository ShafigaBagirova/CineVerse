using Application.Common.Responses;
using Application.Movies.Dtos;
using Domain.Enums;
using MediatR;

namespace Application.Movies.Queries;


public sealed record GetMoviesByStatusQuery(MovieStatus Status,int PageNumber = 1,int PageSize = 10)
 : IRequest<PaginatedResponse<GetAllMoviesResponse>>;

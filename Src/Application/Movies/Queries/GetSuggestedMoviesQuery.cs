using Application.Common.Responses;
using Application.Movies.Dtos;
using MediatR;

namespace Application.Movies.Queries;

public sealed record GetSuggestedMoviesQuery(GetSuggestedMoviesRequest Request)
    : IRequest<BaseResponse<PaginatedResponse<GetSuggestedMoviesResponse>>>;
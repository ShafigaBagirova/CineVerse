using Application.Common.Responses;
using Application.Movies.Dtos;
using Domain.Enums;
using MediatR;

namespace Application.Movies.Queries;

public sealed record GetAllMoviesQuery(GetAllMoviesRequest Request)
    : IRequest<BaseResponse<PaginatedResponse<GetAllMoviesResponse>>>;
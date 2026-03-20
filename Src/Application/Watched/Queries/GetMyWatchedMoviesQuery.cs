using Application.Common.Responses;
using Application.Watched.Dtos;
using MediatR;

namespace Application.Watched.Queries;

public record GetMyWatchedMoviesQuery(int Page=1, int PageSize=10)
    : IRequest<BaseResponse<PaginatedResponse<WatchedMovieDto>>>;

using Application.Common.Responses;
using Application.WatchListItems.Dtos;
using MediatR;

namespace Application.WatchListItems.Queries;

public record GetMyWatchlistQuery(
    int Page = 1,
    int PageSize = 10)
    : IRequest<BaseResponse<PaginatedResponse<WatchlistMovieDto>>>;

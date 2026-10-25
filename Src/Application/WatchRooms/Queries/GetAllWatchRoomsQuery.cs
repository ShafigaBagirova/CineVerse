using Application.Common.Responses;
using Application.WatchRooms.Dtos;
using MediatR;

namespace Application.WatchRooms.Queries;

public sealed record GetAllWatchRoomsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? Search = null,
    string? SortBy = null,
    bool Desc = true)
    : IRequest<BaseResponse<PaginatedResponse<GetAllWatchRoomsResponse>>>;

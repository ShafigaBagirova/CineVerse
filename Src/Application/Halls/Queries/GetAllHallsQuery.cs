using Application.Common.Responses;
using Application.Halls.Dtos;
using MediatR;

namespace Application.Halls.Queries;

public record GetAllHallsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    int? CinemaId = null,
    string? Search = null,
    string? SortBy = null,
    bool Desc = false
) : IRequest<BaseResponse<PaginatedResponse<GetAllHallsResponse>>>;
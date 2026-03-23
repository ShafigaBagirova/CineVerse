using Application.Common.Responses;
using Application.Halls.Dtos;
using MediatR;

namespace Application.Halls.Queries;

public sealed record GetAllHallsQuery(GetAllHallsRequest Request)
    : IRequest<BaseResponse<PaginatedResponse<GetAllHallsResponse>>>;
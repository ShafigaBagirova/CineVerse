using Application.Common.Responses;
using Application.Seats.Dtos;
using MediatR;

namespace Application.Seats.Queries;

public sealed record GetAllSeatsQuery(GetAllSeatsRequest Request)
    : IRequest<BaseResponse<PaginatedResponse<GetAllSeatsResponse>>>;
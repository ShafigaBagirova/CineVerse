using Application.Common.Responses;
using Application.SeatHolds.Dtos;
using MediatR;

namespace Application.SeatHolds.Queries;

public sealed record GetAllSeatHoldsQuery(GetAllSeatHoldsRequest Request)
    : IRequest<BaseResponse<PaginatedResponse<GetAllSeatHoldsResponse>>>;

using Application.Common.Responses;
using Application.SeatHolds.Dtos;
using MediatR;

namespace Application.SeatHolds.Queries;

public sealed record GetSeatHoldsByScreeningQuery(
    int ScreeningId,
    GetSeatHoldsByScreeningRequest Request)
    : IRequest<BaseResponse<PaginatedResponse<GetSeatHoldsByScreeningResponse>>>;
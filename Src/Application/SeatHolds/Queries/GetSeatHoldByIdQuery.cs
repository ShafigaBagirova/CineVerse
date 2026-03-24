using Application.Common.Responses;
using Application.SeatHolds.Dtos;
using MediatR;

namespace Application.SeatHolds.Queries;

public sealed record GetSeatHoldByIdQuery(int Id)
    : IRequest<BaseResponse<GetSeatHoldByIdResponse>>;
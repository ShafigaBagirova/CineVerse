using Application.Common.Responses;
using Application.SeatHolds.Dtos;
using MediatR;

namespace Application.SeatHolds.Commands;

public sealed record CreateSeatHoldCommand(CreateSeatHoldRequest Request) : IRequest<BaseResponse>;
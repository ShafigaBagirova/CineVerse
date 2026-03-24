using Application.Common.Responses;
using MediatR;

namespace Application.SeatHolds.Commands;

public sealed record ReleaseSeatHoldCommand(int Id) : IRequest<BaseResponse>;
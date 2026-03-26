using Application.Common.Responses;
using Application.Payments.Dtos;
using MediatR;

namespace Application.Payments.Commands;

public sealed record RetryPaymentCommand(int SeatHoldId)
    : IRequest<BaseResponse<RetryPaymentResponse>>;
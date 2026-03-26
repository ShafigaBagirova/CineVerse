using Application.Common.Responses;
using Application.Payments.Dtos;
using MediatR;

namespace Application.Payments.Queries;


public sealed record GetPaymentStatusBySeatHoldIdQuery(int SeatHoldId)
    : IRequest<BaseResponse<GetPaymentStatusBySeatHoldIdResponse>>;

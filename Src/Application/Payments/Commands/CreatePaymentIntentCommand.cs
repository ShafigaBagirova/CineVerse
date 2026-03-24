using Application.Common.Responses;
using Application.Payments.Dtos;
using MediatR;

namespace Application.Payments.Commands;

public sealed record CreatePaymentIntentCommand(CreatePaymentIntentRequest Request)
    : IRequest<BaseResponse<CreatePaymentIntentResponse>>;
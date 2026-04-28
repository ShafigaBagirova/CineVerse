using Application.Common.Responses;
using Application.Payments.Dtos;
using MediatR;

namespace Application.Payments.Commands;

public sealed record CreateVipPaymentIntentCommand : IRequest<BaseResponse<CreateVipPaymentIntentResponse>>;

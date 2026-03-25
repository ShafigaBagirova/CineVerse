using Application.Common.Responses;
using MediatR;

namespace Infrastructure.Payments;

public sealed record ProcessStripeWebhookCommand(string Json, string Signature)
    : IRequest<BaseResponse>;
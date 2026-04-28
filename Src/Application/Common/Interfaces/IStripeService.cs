using Application.Payments.Dtos;

namespace Application.Common.Interfaces;

public interface IStripeService
{
    Task<StripePaymentIntentResult> CreatePaymentIntentAsync(decimal amount,string currency,string idempotencyKey,CancellationToken cancellationToken);

    Task<StripePaymentIntentResult> CreateVipPaymentIntentAsync(
        decimal amount,
        string currency,
        string userId,
        string idempotencyKey,
        CancellationToken cancellationToken);

    Task CreateRefundAsync(string providerPaymentIntentId,string idempotencyKey,CancellationToken cancellationToken);
}

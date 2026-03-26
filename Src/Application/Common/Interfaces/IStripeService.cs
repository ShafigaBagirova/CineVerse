using Application.Payments.Dtos;

namespace Application.Common.Interfaces;

public interface IStripeService
{
    Task<StripePaymentIntentResult> CreatePaymentIntentAsync(decimal amount,string currency,string idempotencyKey,CancellationToken cancellationToken);
}

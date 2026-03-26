using Application.Common.Interfaces;
using Application.Common.Options;
using Application.Payments.Dtos;
using Microsoft.Extensions.Options;
using Stripe;

namespace Infrastructure.Payments;

public sealed class StripeService : IStripeService
{
    private readonly StripeSettings _settings;

    public StripeService(IOptions<StripeSettings> settings)
    {
        _settings = settings.Value;
        StripeConfiguration.ApiKey = _settings.SecretKey;
    }

    public async Task<StripePaymentIntentResult> CreatePaymentIntentAsync(
        decimal amount,
        string currency,
        string idempotencyKey, 
        CancellationToken cancellationToken)
    {
        try
        {
            var service = new PaymentIntentService();

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)Math.Round(amount * 100, MidpointRounding.AwayFromZero),

                Currency = currency.ToLower(),

                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                }
            };

            var requestOptions = new RequestOptions
            {
                IdempotencyKey = idempotencyKey
            };

            var intent = await service.CreateAsync(
                options,
                requestOptions,
                cancellationToken);

            return new StripePaymentIntentResult
            {
                PaymentIntentId = intent.Id,
                ClientSecret = intent.ClientSecret
            };
        }
        catch (StripeException ex)
        {
            throw new Exception($"Stripe error: {ex.Message}", ex);
        }
    }
}
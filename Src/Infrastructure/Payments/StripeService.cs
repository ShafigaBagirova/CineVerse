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
     CancellationToken cancellationToken)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount * 100),
            Currency = currency,
            PaymentMethodTypes = new List<string> { "card" }
        };

        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(options, cancellationToken: cancellationToken);

        return new StripePaymentIntentResult
        {
            PaymentIntentId = intent.Id,
            ClientSecret = intent.ClientSecret
        };
    }
}
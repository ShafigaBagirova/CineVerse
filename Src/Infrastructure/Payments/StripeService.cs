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
            var amountInSmallestUnit = (long)(amount * 100);
            var options = new PaymentIntentCreateOptions
            {
                Amount = amountInSmallestUnit,
                Currency = currency,
                PaymentMethodTypes = new List<string> { "card" }
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

    public async Task<StripePaymentIntentResult> CreateVipPaymentIntentAsync(
        decimal amount,
        string currency,
        string userId,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        try
        {
            var service = new PaymentIntentService();
            var amountInSmallestUnit = (long)(amount * 100);
            var options = new PaymentIntentCreateOptions
            {
                Amount = amountInSmallestUnit,
                Currency = currency,
                PaymentMethodTypes = new List<string> { "card" },
                Metadata = new Dictionary<string, string>
                {
                    ["cineverse_kind"] = "vip",
                    ["user_id"] = userId
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

    public async Task CreateRefundAsync(
        string providerPaymentIntentId,
        string idempotencyKey,
        CancellationToken cancellationToken)
    {
        var refundService = new RefundService();

        var options = new RefundCreateOptions
        {
            PaymentIntent = providerPaymentIntentId
        };

        var requestOptions = new RequestOptions
        {
            IdempotencyKey = idempotencyKey
        };

        await refundService.CreateAsync(
            options,
            requestOptions,
            cancellationToken);
    }
}
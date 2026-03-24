namespace Application.Payments.Dtos;

public sealed class StripePaymentIntentResult
{
    public string PaymentIntentId { get; set; } = default!;
    public string ClientSecret { get; set; } = default!;
}
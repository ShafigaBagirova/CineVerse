namespace Application.Payments.Dtos;

public sealed class CreatePaymentIntentResponse
{
    public int PaymentId { get; set; }
    public int SeatHoldId { get; set; }
    public decimal Amount { get; set; }
    public string ClientSecret { get; set; } = default!;
    public string ProviderPaymentIntentId { get; set; } = default!;
    public string Status { get; set; } = default!;
}
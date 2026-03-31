using Domain.Enums;

namespace Application.Payments.Dtos;

public sealed class RetryPaymentResponse
{
    public int PaymentId { get; set; }
    public int SeatHoldId { get; set; }
    public PaymentProvider Provider { get; set; } = default!;
    public string ProviderPaymentIntentId { get; set; } = default!;
    public string ClientSecret { get; set; } = default!;
    public decimal TotalAmount { get; set; }
    public decimal TicketAmount { get; set; }
    public decimal FoodAmount { get; set; }
    public string Currency { get; set; } = default!;
    public PaymentStatus Status { get; set; } = default!;
}
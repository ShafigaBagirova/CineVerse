using Domain.Enums;

namespace Domain.Entities;

public class Payment : BaseEntity<int>
{
    public int SeatHoldId { get; set; }
    public string? UserId { get; set; } = default!;
    public decimal TicketAmount { get; set; }
    public decimal FoodAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public PaymentStatus Status { get; set; }
    public PaymentProvider Provider { get; set; }
    public string ProviderPaymentIntentId { get; set; } = default!;
    public string? ClientSecret { get; set; }
    public DateTime? PaidAtUtc { get; set; }
    public SeatHold SeatHold { get; set; } = default!;
    public PaymentCurrency Currency { get; set; }
    public DateTime? RefundedAtUtc { get; set; }
}
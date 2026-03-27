using Domain.Enums;

namespace Application.Payments.Dtos;

public sealed class GetAllPaymentsResponse
{
    public int Id { get; set; }
    public int  SeatHoldId { get; set; }
    public string UserId { get; set; } = default!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = default!;
    public PaymentStatus Status { get; set; } = default!;
    public PaymentProvider Provider { get; set; } = default!;
    public string? ProviderPaymentIntentId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? PaidAtUtc { get; set; }
    public DateTime? RefundedAtUtc { get; set; }
}
using Domain.Enums;

namespace Application.Payments.Dtos;

public sealed class GetPaymentByIdResponse
{
    public int Id { get; set; }
    public int SeatHoldId { get; set; }
    public PaymentProvider Provider { get; set; } = default!;
    public PaymentStatus Status { get; set; } = default!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = default!;
    public string? ProviderPaymentIntentId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? PaidAtUtc { get; set; }
}
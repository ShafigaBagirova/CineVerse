using Domain.Enums;

namespace Application.Payments.Dtos;

public sealed class GetPaymentStatusBySeatHoldIdResponse
{
    public int SeatHoldId { get; set; }
    public bool HasPayment { get; set; }
    public PaymentStatus Status { get; set; } = default!;
    public decimal? Amount { get; set; }
    public string? Currency { get; set; }
    public string? PaymentIntentId { get; set; }
    public DateTime? PaidAtUtc { get; set; }
}
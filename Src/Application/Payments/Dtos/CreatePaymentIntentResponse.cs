using Domain.Enums;

namespace Application.Payments.Dtos;

public sealed class CreatePaymentIntentResponse
{
    public int PaymentId { get; set; }
    public int SeatHoldId { get; set; }
    public decimal TicketAmount { get; set; }
    public decimal FoodAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string ClientSecret { get; set; } = default!;
    public string ProviderPaymentIntentId { get; set; } = default!;
    public PaymentStatus Status { get; set; } = default!;
}
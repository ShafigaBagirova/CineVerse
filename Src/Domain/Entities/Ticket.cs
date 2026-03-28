using Domain.Enums;

namespace Domain.Entities;

public class Ticket : BaseEntity<int>
{
    public int ScreeningId { get; set; }
    public int SeatId { get; set; }
    public string UserId { get; set; }= default!;
    public decimal Price { get; set; }
    public TicketStatus Status { get; set; }
    public DateTime PurchasedAtUtc { get; set; }
    public Screening Screening { get; set; } = default!;
    public Seat Seat { get; set; } = default!;
    public PaymentCurrency Currency { get; set; }

}
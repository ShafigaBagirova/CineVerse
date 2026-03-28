using Domain.Enums;

namespace Domain.Entities;

public class FoodOrder :BaseAuditableEntity
{
    public string UserId { get; set; } = default!;
    public int SeatHoldId { get; set; }
    public SeatHold SeatHold { get; set; } = default!;
    public int ScreeningId { get; set; }
    public Screening Screening { get; set; } = default!;
    public int SeatId { get; set; }
    public Seat Seat { get; set; } = default!;
    public int CinemaId { get; set; }
    public Cinema Cinema { get; set; } = default!;
    public decimal TotalAmount { get; set; }
    public DeliveryType DeliveryType { get; set; } = DeliveryType.SeatDelivery;
    public FoodOrderStatus Status { get; set; } = FoodOrderStatus.Pending;
    public string? Note { get; set; }
    public DateTime? ConfirmedAtUtc { get; set; }
    public DateTime? DeliveredAtUtc { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public DateTime? RefundedAtUtc { get; set; }
    public ICollection<FoodOrderItem> FoodOrderItems { get; set; } = new List<FoodOrderItem>();
}
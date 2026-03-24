using Domain.Enums;

namespace Domain.Entities;

public class SeatHold: BaseEntity<int>
{
    public int ScreeningId { get; set; }
    public Screening Screening { get; set; } = null!;
    public int SeatId { get; set; }
    public Seat Seat { get; set; } = null!;
    public int UserId { get; set; }
    public SeatHoldStatus Status { get; set; }
    public DateTime ExpiresAtUtc { get; set; }

}

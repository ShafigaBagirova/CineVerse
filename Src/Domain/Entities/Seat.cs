using Domain.Enums;

namespace Domain.Entities;

public class Seat : BaseEntity<int>
{
    public int HallId { get; set; }
    public string Row { get; set; } = default!;
    public int Number { get; set; }
    public SeatType Type { get; set; } = SeatType.Standard;
    public bool IsActive { get; set; } = true;
    public Hall Hall { get; set; } = default!;
    public ICollection<SeatHold> SeatHolds { get; set; } = new List<SeatHold>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
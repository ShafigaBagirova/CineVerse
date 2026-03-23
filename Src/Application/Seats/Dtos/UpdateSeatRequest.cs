using Domain.Enums;

namespace Application.Seats.Dtos;

public class UpdateSeatRequest
{
    public int? HallId { get; set; }
    public string? Row { get; set; } = default!;
    public int? Number { get; set; }
    public SeatType? Type { get; set; } = SeatType.Standard;
    public bool? IsActive { get; set; }
}

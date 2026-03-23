using Domain.Enums;

namespace Application.Seats.Dtos;

public class CreateSeatRequest
{
    public int HallId { get; set; }
    public string Row { get; set; } = default!;
    public int Number { get; set; }
    public SeatType Type { get; set; } = SeatType.Standard;
}

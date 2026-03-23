using Domain.Enums;

namespace Application.Seats.Dtos;

public class GetSeatsByScreeningResponse
{
    public int SeatId { get; set; }
    public int HallId { get; set; }

    public string Row { get; set; } = default!;
    public int Number { get; set; }

    public SeatType Type { get; set; }
    public bool IsActive { get; set; }

    public SeatAvailabilityStatus Status { get; set; }
}
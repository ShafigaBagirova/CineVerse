using Domain.Enums;

namespace Application.Seats.Dtos;

public class GetAllSeatsResponse
{
    public int Id { get; set; }
    public int HallId { get; set; }
    public string Row { get; set; } = default!;
    public int Number { get; set; }
    public SeatType Type { get; set; }
    public bool IsActive { get; set; }
}
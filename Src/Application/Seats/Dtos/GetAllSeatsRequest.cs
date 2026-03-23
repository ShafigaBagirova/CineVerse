using Domain.Enums;

namespace Application.Seats.Dtos;

public class GetAllSeatsRequest
{
    public int? HallId { get; set; }
    public string? Row { get; set; }
    public SeatType? Type { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

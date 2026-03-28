namespace Application.AdminDashboard.Dtos;

public sealed class ScreeningOccupancyDto
{
    public int ScreeningId { get; set; }
    public string MovieTitle { get; set; } = default!;
    public string CinemaName { get; set; } = default!;
    public string HallName { get; set; } = default!;
    public DateTime StartTime { get; set; }
    public int TotalSeats { get; set; }
    public int SoldSeats { get; set; }
    public double OccupancyRate { get; set; }
}
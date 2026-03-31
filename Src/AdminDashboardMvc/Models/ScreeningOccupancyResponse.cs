namespace AdminDashboardMvc.Models;

public class ScreeningOccupancyResponse
{
    public int ScreeningId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string HallName { get; set; } = string.Empty;
    public decimal OccupancyRate { get; set; }
}
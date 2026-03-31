namespace AdminDashboardMvc.Models;

public class TopMovieResponse
{
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public int TicketsSold { get; set; }
    public decimal Revenue { get; set; }
}
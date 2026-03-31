namespace AdminDashboardMvc.Models;

public class DashboardSummaryResponse
{
    public decimal TotalRevenue { get; set; }
    public int TotalUsers { get; set; }
    public int TotalMovies { get; set; }
    public int TotalScreenings { get; set; }
    public int TotalTickets { get; set; }
    public int TotalPayments { get; set; }
}
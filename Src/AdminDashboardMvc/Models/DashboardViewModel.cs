namespace AdminDashboardMvc.Models;

public class DashboardViewModel
{
    public DashboardSummaryResponse? Summary { get; set; }
    public List<RevenueChartItemResponse> RevenueChart { get; set; } = new();
    public List<TopMovieResponse> TopMovies { get; set; } = new();
    public List<RecentPaymentResponse> RecentPayments { get; set; } = new();
    public List<RecentUserResponse> RecentUsers { get; set; } = new();
    public List<ScreeningOccupancyResponse> ScreeningOccupancies { get; set; } = new();
}
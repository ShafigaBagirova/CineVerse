namespace Application.AdminDashboard.Dtos;

public sealed class AdminDashboardSummaryDto
{
    public int TotalUsers { get; set; }
    public int VipUsers { get; set; }
    public int TotalMovies { get; set; }
    public int TotalCinemas { get; set; }
    public int ActiveScreenings { get; set; }
    public int TotalTicketsSold { get; set; }
    public decimal TotalRevenue { get; set; }
    public int SuccessfulPayments { get; set; }
    public int FailedPayments { get; set; }
    public int PendingPayments { get; set; }
    public int TotalReviews { get; set; }
    public int TotalRatings { get; set; }
    public int TotalWatchlistItems { get; set; }
}
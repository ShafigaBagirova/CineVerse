namespace Application.AdminDashboard.Dtos;

public sealed class TopMovieDto
{
    public int MovieId { get; set; }
    public string Title { get; set; } = default!;
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int WatchlistCount { get; set; }
    public int WatchCount { get; set; }
    public int TicketCount { get; set; }
    public decimal Revenue { get; set; }
}
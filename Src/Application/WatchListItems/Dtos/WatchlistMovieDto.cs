namespace Application.WatchListItems.Dtos;

public class WatchlistMovieDto
{
    public int MovieId { get; set; }
    public string Title { get; set; } = default!;
    public string? PosterUrl { get; set; }
    public decimal? UserAverageRating { get; set; }
    public DateTime AddedAt { get; set; }
}
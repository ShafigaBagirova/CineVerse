namespace Application.Watched.Dtos;

public class WatchedMovieDto
{
    public int MovieId { get; set; }
    public string Title { get; set; } = default!;
    public string? PosterPath { get; set; }
    public decimal? UserAverageRating { get; set; }
    public DateTime CreatedAt { get; set; }
}
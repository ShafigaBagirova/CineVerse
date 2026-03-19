namespace Application.Movies.Dtos;

public class GetAllMoviesResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? PosterUrl { get; set; }
    public string? BackdropUrl { get; set; }
    public decimal? UserAverageRating { get; set; }
}
namespace Application.Movies.Dtos;

public class GetAllMoviesResponse
{
    public int Id { get; set; }
    public int TmdbId { get; set; }

    public string Title { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string? Description { get; set; }
    public string? Director { get; set; }
    public string? Actors { get; set; }

    public string? PosterUrl { get; set; }
    public string? BackdropUrl { get; set; }

    public string? Language { get; set; }
    public string? Country { get; set; }

    public int? ReleaseYear { get; set; }
    public DateOnly? ReleaseDate { get; set; }

    public decimal? TmdbRating { get; set; }
    public decimal? UserAverageRating { get; set; }

    public string? Status { get; set; }
    public int DurationMinutes { get; set; }
}
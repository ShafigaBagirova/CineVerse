using Domain.Enums;

namespace Application.Movies.Dtos;

public sealed class ExternalMovieDto
{
    public long ExternalId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? Country { get; set; }
    public string? AgeRating { get; set; }
    public string? Tagline { get; set; }
    public DateOnly? ReleaseDate { get; set; }
    public string? Director { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Language { get; set; }
    public string Slug { get; set; } = null!;
    public MovieStatus Status { get; set; }
    public decimal? TmdbRating { get; set; }
    public string? PosterPath { get; set; }
    public string? BackdropPath { get; set; }
}

using Domain.Enums;

namespace Application.Movies.Dtos;

public sealed class UpdateMovieRequest
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? Country { get; set; }
    public string? AgeRating { get; set; }
    public string? Tagline { get; set; }
    public DateOnly? ReleaseDate { get; set; }
    public string? Director { get; set; }
    public int DurationMinutes { get; set; }
    public string? Language { get; set; }
    public MovieStatus Status { get; set; }
}

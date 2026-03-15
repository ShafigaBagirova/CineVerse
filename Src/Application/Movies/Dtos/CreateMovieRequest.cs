using Domain.Enums;

namespace Application.Movies.Dtos;

public sealed class CreateMovieRequest
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Country { get; set; }=null!;
    public string AgeRating { get; set; }=null!;
    public string Tagline { get; set; } = null!;
    public DateOnly ReleaseDate { get; set; }
    public string Director { get; set; }= null!;
    public int DurationMinutes { get; set; }=0!;
    public string Language { get; set; } = null!;
    public long TmdbId { get; set; }
}

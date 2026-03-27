namespace Application.Movies.Dtos;

public sealed class GetSuggestedMoviesResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string? PosterUrl { get; set; }
    public decimal ImdbRating { get; set; }
    public int ReleaseDate { get; set; }
    public string Slug { get; set; } = default!;
    public List<string> Genres { get; set; } = [];
}
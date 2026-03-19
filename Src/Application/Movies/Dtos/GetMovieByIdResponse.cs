namespace Application.Movies.Dtos;

public class GetMovieByIdResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; }=null!;
    public string? PosterUrl { get; set; }
    public string? BackdropUrl { get; set; }
    public decimal? UserAverageRating { get; set; }
    public decimal? MyRating { get; set; }
    public int RatingCount { get; set; }
    public List<ReviewDto> Reviews { get; set; } = new();
    public List<MovieGenreDto> Genres { get; set; } = new();
}
namespace Application.Movies.Dtos;

public class CreateMovieGenreRequest
{
    public int GenreId { get; set; }
    public bool IsPrimary { get; set; }
    public int Order { get; set; }
}
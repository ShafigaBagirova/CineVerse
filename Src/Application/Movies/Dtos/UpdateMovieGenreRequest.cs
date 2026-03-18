namespace Application.Movies.Dtos;

public class UpdateMovieGenreRequest
{
    public bool IsPrimary { get; set; }
    public int Order { get; set; }
}
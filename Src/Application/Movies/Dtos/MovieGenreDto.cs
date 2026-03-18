namespace Application.Movies.Dtos;

public class MovieGenreDto
{
    public int GenreId { get; set; }
    public string Name { get; set; } = default!;
    public bool IsPrimary { get; set; }
    public int Order { get; set; }
}

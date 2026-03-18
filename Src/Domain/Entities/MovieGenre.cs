namespace Domain.Entities;

public class MovieGenre
{
    public int MovieId { get; set; }
    public Movie Movie { get; set; } = default!;

    public int GenreId { get; set; }
    public Genre Genre { get; set; } = default!;

    public bool IsPrimary { get; set; }
    public int Order { get; set; }
}

namespace Domain.Entities;

public class MoviePoster
{
    public int Id { get; set; }
    public int Order { get; set; }
    public string ObjectKey { get; set; } = null!;
    public int MovieId { get; set; }
    public Movie Movie { get; set; } = null!;
}

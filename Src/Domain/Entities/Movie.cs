namespace Domain.Entities;

public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int Year { get; set; }
    public string? Director { get; set; }

    public int DurationMinutes { get; set; }

    public string? Language { get; set; }

    public decimal? ImdbRating { get; set; }

    public DateTime CreatedAt { get; set; }
    public ICollection<MoviePoster> MediaItems { get; set; } = new List<MoviePoster>();
}

using Domain.Enums;

namespace Domain.Entities;

public class Movie: BaseAuditableEntity
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? Country { get; set; }
    public string? AgeRating { get; set; }
    public string? Tagline { get; set; }
    public DateOnly? ReleaseDate { get; set; }
    public string? Director { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Language { get; set; }
    public decimal? ImdbRating { get; set; }
    public string Slug { get; set; } = null!;
    public MovieStatus Status { get; set; }
    public long? TmdbId { get; set; }
    public decimal? TmdbRating { get; set; }
    public decimal? UserAverageRating { get; set; }
    public int RatingCount { get; set; }
    public string? PosterPath { get; set; }
    public string? BackdropPath { get; set; }
    public ICollection<MovieVideo> Videos { get; set; } = new List<MovieVideo>();
    public ICollection<MovieRating> MovieRatings { get; set; } = new List<MovieRating>();
    public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<WatchListItem> WatchlistItems { get; set; } = new List<WatchListItem>();
    public ICollection<WatchLog> WatchLogs { get; set; } = new List<WatchLog>();
}

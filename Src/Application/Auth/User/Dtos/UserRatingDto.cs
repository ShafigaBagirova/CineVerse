namespace Application.Auth.User.Dtos;

public class UserRatingDto
{
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = default!;
    public string? PosterUrl { get; set; }
    public decimal Rating { get; set; }
    public DateTime CreatedAt { get; set; }
}
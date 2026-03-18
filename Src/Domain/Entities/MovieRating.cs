namespace Domain.Entities;

public class MovieRating:BaseEntity<int>
{
    public int MovieId { get; set; }
    public Movie Movie { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public decimal Rating { get; set; } 
}

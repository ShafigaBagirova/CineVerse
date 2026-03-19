namespace Application.Movies.Dtos;

public class UpdateReviewRequest
{
    public string Content { get; set; } = default!;
    public bool IsSpoiler { get; set; }
}
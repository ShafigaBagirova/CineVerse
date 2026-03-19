namespace Application.Movies.Dtos;

public class CreateReviewRequest
{
    public string Content { get; set; } = default!;
    public bool IsSpoiler { get; set; }
}

namespace Application.Reviews.Dtos;

public class ReviewDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
    public string? UserName { get; set; }
    public string Content { get; set; } = default!;
    public bool IsEdited { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsSpoiler { get; set; }
}
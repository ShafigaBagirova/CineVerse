namespace Domain.Entities;

public class Review:BaseAuditableEntity
{
    public int MovieId { get; set; }
    public Movie Movie { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public string Content { get; set; } = default!;
    public string? UserName { get; set; } 
    public bool IsEdited { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsSpoiler { get; set; }
}

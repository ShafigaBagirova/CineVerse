namespace Domain.Entities;

public class Message : BaseEntity<int>
{
    public int ChatId { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public Chat Chat { get; set; } = default!;
}

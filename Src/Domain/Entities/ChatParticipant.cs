namespace Domain.Entities;

public class ChatParticipant : BaseEntity<int>
{
    public int ChatId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public Chat Chat { get; set; } = default!;
}

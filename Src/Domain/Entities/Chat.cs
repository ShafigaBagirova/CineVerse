namespace Domain.Entities;

public class Chat : BaseEntity<int>
{
    public DateTime CreatedAt { get; set; }
    public bool IsGroup { get; set; }
    public ICollection<ChatParticipant> Participants { get; set; } = new List<ChatParticipant>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}

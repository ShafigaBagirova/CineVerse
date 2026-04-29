namespace Application.Chats.Dtos;

public sealed class ChatDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsGroup { get; set; }
    public List<ChatParticipantDto> Participants { get; set; } = [];
    public MessageDto? LastMessage { get; set; }
    public int UnreadCount { get; set; }
}

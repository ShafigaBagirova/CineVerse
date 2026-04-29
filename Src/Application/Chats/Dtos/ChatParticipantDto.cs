namespace Application.Chats.Dtos;

public sealed class ChatParticipantDto
{
    public int Id { get; set; }
    public int ChatId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public DateTime JoinedAt { get; set; }
}

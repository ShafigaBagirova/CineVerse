namespace Domain.Entities;

public class WatchRoomMember : BaseEntity<int>
{
    public int WatchRoomId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public WatchRoom WatchRoom { get; set; } = default!;
}

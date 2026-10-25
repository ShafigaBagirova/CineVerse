using Domain.Enums;

namespace Domain.Entities;

public class WatchRoom : BaseEntity<int>
{
    public string Code { get; set; } = string.Empty;
    public string HostUserId { get; set; } = string.Empty;
    public WatchPlatform Platform { get; set; }
    public string ContentId { get; set; } = string.Empty;
    public string ContentUrl { get; set; } = string.Empty;
    public string? Title { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public ICollection<WatchRoomMember> Members { get; set; } = new List<WatchRoomMember>();
}

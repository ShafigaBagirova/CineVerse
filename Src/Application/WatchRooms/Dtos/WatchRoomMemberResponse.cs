namespace Application.WatchRooms.Dtos;

public class WatchRoomMemberResponse
{
    public string UserId { get; set; } = default!;
    public string? UserName { get; set; }
    public DateTime JoinedAt { get; set; }
}

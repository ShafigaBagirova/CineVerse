namespace Application.WatchRooms.Dtos;

public class GetAllWatchRoomsResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = default!;
    public string HostUserId { get; set; } = default!;
    public string Platform { get; set; } = default!;
    public string ContentUrl { get; set; } = default!;
    public string? Title { get; set; }
    public int MemberCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

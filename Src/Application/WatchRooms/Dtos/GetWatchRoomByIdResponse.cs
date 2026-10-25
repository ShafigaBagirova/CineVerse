namespace Application.WatchRooms.Dtos;

public class GetWatchRoomByIdResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = default!;
    public string HostUserId { get; set; } = default!;
    public string Platform { get; set; } = default!;
    public string ContentId { get; set; } = default!;
    public string ContentUrl { get; set; } = default!;
    public string? Title { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<WatchRoomMemberResponse> Members { get; set; } = new();
}

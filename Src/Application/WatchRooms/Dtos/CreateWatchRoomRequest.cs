namespace Application.WatchRooms.Dtos;

public class CreateWatchRoomRequest
{
    public string ContentUrl { get; set; } = default!;
    public string? Title { get; set; }
    public double? StartPositionSeconds { get; set; }
    public bool StartPlaying { get; set; }
}

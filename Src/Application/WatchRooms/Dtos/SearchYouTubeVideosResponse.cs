namespace Application.WatchRooms.Dtos;

public class SearchYouTubeVideosResponse
{
    public string VideoId { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string ChannelTitle { get; set; } = default!;
    public string ThumbnailUrl { get; set; } = default!;
}

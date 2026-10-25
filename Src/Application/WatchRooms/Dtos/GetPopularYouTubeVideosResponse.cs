namespace Application.WatchRooms.Dtos;

public class GetPopularYouTubeVideosResponse
{
    public List<SearchYouTubeVideosResponse> Items { get; set; } = [];

    /// <summary>Pass this back to get the next page; null when there are no more videos.</summary>
    public string? NextPageToken { get; set; }
}

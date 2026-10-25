namespace Application.Common.Options;

public sealed class YouTubeOptions
{
    public const string SectionName = "YouTube";

    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://www.googleapis.com/youtube/v3/";
    public int MaxResults { get; set; } = 12;
}

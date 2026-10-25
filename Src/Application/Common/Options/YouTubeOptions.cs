namespace Application.Common.Options;

public sealed class YouTubeOptions
{
    public const string SectionName = "YouTube";

    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://www.googleapis.com/youtube/v3/";
    public int MaxResults { get; set; } = 12;

    /// <summary>Country whose most popular videos are shown before anyone searches (two letters, for example US).</summary>
    public string PopularRegionCode { get; set; } = "US";
    /// <summary>Videos asked for per page of the popular list (some may be dropped because they cannot be embedded).</summary>
    public int PopularMaxResults { get; set; } = 24;
}

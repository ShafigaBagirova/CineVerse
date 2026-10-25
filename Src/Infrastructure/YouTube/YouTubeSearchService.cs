using Application.Common.Interfaces;
using Application.Common.Options;
using Application.WatchRooms.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;

namespace Infrastructure.YouTube;

public sealed class YouTubeSearchService : IYouTubeSearchService
{
    private readonly HttpClient _httpClient;
    private readonly YouTubeOptions _options;
    private readonly ILogger<YouTubeSearchService> _logger;

    public YouTubeSearchService(
        HttpClient httpClient,
        IOptions<YouTubeOptions> options,
        ILogger<YouTubeSearchService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_options.ApiKey) &&
        !_options.ApiKey.StartsWith("REPLACE_WITH_", StringComparison.Ordinal);

    public async Task<List<SearchYouTubeVideosResponse>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        var url =
            $"search?part=snippet&type=video&videoEmbeddable=true&safeSearch=moderate" +
            $"&maxResults={_options.MaxResults}" +
            $"&q={Uri.EscapeDataString(query)}" +
            $"&key={Uri.EscapeDataString(_options.ApiKey)}";

        using var response = await _httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "YouTube search failed. StatusCode: {StatusCode}",
                (int)response.StatusCode);

            throw new HttpRequestException(
                $"YouTube search failed with status {(int)response.StatusCode}.",
                null,
                response.StatusCode);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var results = new List<SearchYouTubeVideosResponse>();

        if (!document.RootElement.TryGetProperty("items", out var items))
            return results;

        foreach (var item in items.EnumerateArray())
        {
            if (!item.TryGetProperty("id", out var id) ||
                !id.TryGetProperty("videoId", out var videoId) ||
                !item.TryGetProperty("snippet", out var snippet))
                continue;

            results.Add(new SearchYouTubeVideosResponse
            {
                VideoId = videoId.GetString() ?? string.Empty,
                Title = WebUtility.HtmlDecode(GetString(snippet, "title")),
                ChannelTitle = WebUtility.HtmlDecode(GetString(snippet, "channelTitle")),
                ThumbnailUrl = GetThumbnailUrl(snippet)
            });
        }

        return results.Where(r => r.VideoId.Length > 0).ToList();
    }

    private static string GetString(JsonElement element, string name)
        => element.TryGetProperty(name, out var value) ? value.GetString() ?? string.Empty : string.Empty;

    private static string GetThumbnailUrl(JsonElement snippet)
    {
        if (!snippet.TryGetProperty("thumbnails", out var thumbnails))
            return string.Empty;

        foreach (var size in new[] { "medium", "high", "default" })
        {
            if (thumbnails.TryGetProperty(size, out var thumbnail) &&
                thumbnail.TryGetProperty("url", out var url))
                return url.GetString() ?? string.Empty;
        }

        return string.Empty;
    }
}

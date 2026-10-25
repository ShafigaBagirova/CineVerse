using Application.Common.Interfaces;
using Application.Common.Options;
using Application.WatchRooms.Dtos;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;

namespace Infrastructure.YouTube;

public sealed class YouTubeSearchService : IYouTubeSearchService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan PopularCacheDuration = TimeSpan.FromMinutes(30);

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly YouTubeOptions _options;
    private readonly ILogger<YouTubeSearchService> _logger;

    public YouTubeSearchService(
        HttpClient httpClient,
        IMemoryCache cache,
        IOptions<YouTubeOptions> options,
        ILogger<YouTubeSearchService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_options.ApiKey) &&
        !_options.ApiKey.StartsWith("REPLACE_WITH_", StringComparison.Ordinal);

    public async Task<List<SearchYouTubeVideosResponse>> SearchAsync(string query, CancellationToken cancellationToken)
    {
        // Every search costs 100 of the 10 000 daily quota units, so the same words are answered from memory for a while.
        var cacheKey = $"youtube-search:{_options.MaxResults}:{query.ToLowerInvariant()}";
        if (_cache.TryGetValue(cacheKey, out List<SearchYouTubeVideosResponse>? cached) && cached is not null)
            return cached;

        var url =
            $"search?part=snippet&type=video&videoEmbeddable=true&safeSearch=moderate" +
            $"&maxResults={_options.MaxResults}" +
            $"&q={Uri.EscapeDataString(query)}";

        using var document = await GetJsonAsync(url, cancellationToken);
        var results = ReadItems(document, idIsObject: true, requireEmbeddable: false, _options.MaxResults);

        _cache.Set(cacheKey, results, CacheDuration);
        return results;
    }

    public async Task<GetPopularYouTubeVideosResponse> GetPopularAsync(string? pageToken, CancellationToken cancellationToken)
    {
        // Listing popular videos costs 1 quota unit per page (a search costs 100), and it changes slowly, so each page is kept for a while.
        var region = string.IsNullOrWhiteSpace(_options.PopularRegionCode) ? "US" : _options.PopularRegionCode.Trim().ToUpperInvariant();
        var pageSize = Math.Clamp(_options.PopularMaxResults, 1, 50);
        var cacheKey = $"youtube-popular:{region}:{pageSize}:{pageToken}";
        if (_cache.TryGetValue(cacheKey, out GetPopularYouTubeVideosResponse? cached) && cached is not null)
            return cached;

        var url =
            $"videos?part=snippet,status&chart=mostPopular&videoCategoryId=0" +
            $"&regionCode={Uri.EscapeDataString(region)}" +
            $"&maxResults={pageSize}";

        if (!string.IsNullOrEmpty(pageToken))
            url += $"&pageToken={Uri.EscapeDataString(pageToken)}";

        using var document = await GetJsonAsync(url, cancellationToken);

        // A page may lose some videos to the embeddable filter; the next page token still follows YouTube's own paging.
        var response = new GetPopularYouTubeVideosResponse
        {
            Items = ReadItems(document, idIsObject: false, requireEmbeddable: true, int.MaxValue),
            NextPageToken = document.RootElement.TryGetProperty("nextPageToken", out var next) ? next.GetString() : null
        };

        _cache.Set(cacheKey, response, PopularCacheDuration);
        return response;
    }

    private async Task<JsonDocument> GetJsonAsync(string url, CancellationToken cancellationToken)
    {
        // The key goes in a header, not in the address, so it can never end up in request logs.
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("X-Goog-Api-Key", _options.ApiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "YouTube request failed. StatusCode: {StatusCode}",
                (int)response.StatusCode);

            throw new HttpRequestException(
                $"YouTube request failed with status {(int)response.StatusCode}.",
                null,
                response.StatusCode);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
    }

    private static List<SearchYouTubeVideosResponse> ReadItems(
        JsonDocument document,
        bool idIsObject,
        bool requireEmbeddable,
        int maxCount)
    {
        var results = new List<SearchYouTubeVideosResponse>();

        if (!document.RootElement.TryGetProperty("items", out var items))
            return results;

        foreach (var item in items.EnumerateArray())
        {
            if (results.Count >= maxCount)
                break;

            if (!item.TryGetProperty("id", out var id) || !item.TryGetProperty("snippet", out var snippet))
                continue;

            string? videoId;
            if (idIsObject)
                videoId = id.TryGetProperty("videoId", out var videoIdElement) ? videoIdElement.GetString() : null;
            else
                videoId = id.ValueKind == JsonValueKind.String ? id.GetString() : null;

            if (string.IsNullOrEmpty(videoId))
                continue;

            if (requireEmbeddable && !IsEmbeddable(item))
                continue;

            results.Add(new SearchYouTubeVideosResponse
            {
                VideoId = videoId,
                Title = WebUtility.HtmlDecode(GetString(snippet, "title")),
                ChannelTitle = WebUtility.HtmlDecode(GetString(snippet, "channelTitle")),
                ThumbnailUrl = GetThumbnailUrl(snippet)
            });
        }

        return results;
    }

    private static bool IsEmbeddable(JsonElement item)
        => item.TryGetProperty("status", out var status) &&
           status.TryGetProperty("embeddable", out var embeddable) &&
           embeddable.ValueKind == JsonValueKind.True;

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

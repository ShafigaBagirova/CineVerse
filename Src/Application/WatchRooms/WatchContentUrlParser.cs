using Domain.Enums;
using System.Text.RegularExpressions;

namespace Application.WatchRooms;

public static class WatchContentUrlParser
{
    private static readonly Regex YouTubeIdPattern = new("^[A-Za-z0-9_-]{11}$", RegexOptions.Compiled);
    private static readonly Regex NetflixIdPattern = new("^[0-9]{3,12}$", RegexOptions.Compiled);
    private static readonly Regex NetflixLocalePattern = new("^[a-z]{2}(-[A-Za-z]{2,4})?$", RegexOptions.Compiled);

    public static bool TryParse(string? url, out WatchPlatform platform, out string contentId)
    {
        platform = default;
        contentId = string.Empty;

        if (string.IsNullOrWhiteSpace(url) ||
            !Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
            return false;

        var host = uri.Host.ToLowerInvariant();

        if (TryParseYouTube(uri, host, out var youTubeId))
        {
            platform = WatchPlatform.YouTube;
            contentId = youTubeId;
            return true;
        }

        if (TryParseNetflix(uri, host, out var netflixId))
        {
            platform = WatchPlatform.Netflix;
            contentId = netflixId;
            return true;
        }

        return false;
    }

    public static string BuildCanonicalUrl(WatchPlatform platform, string contentId)
        => platform switch
        {
            WatchPlatform.YouTube => $"https://www.youtube.com/watch?v={contentId}",
            WatchPlatform.Netflix => $"https://www.netflix.com/watch/{contentId}",
            _ => throw new ArgumentOutOfRangeException(nameof(platform), platform, null)
        };

    private static bool TryParseYouTube(Uri uri, string host, out string contentId)
    {
        contentId = string.Empty;
        string? candidate = null;

        if (host == "youtu.be")
        {
            candidate = uri.AbsolutePath.Trim('/').Split('/')[0];
        }
        else if (host is "youtube.com" or "www.youtube.com" or "m.youtube.com")
        {
            var segments = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 2 && segments[0] is "embed" or "shorts" or "live")
                candidate = segments[1];
            else if (segments.Length == 1 && segments[0] == "watch")
                candidate = GetQueryValue(uri.Query, "v");
        }

        if (candidate is null || !YouTubeIdPattern.IsMatch(candidate))
            return false;

        contentId = candidate;
        return true;
    }

    // Accepts netflix.com/watch/<id> with an optional locale prefix such as /tr/watch/<id>.
    private static bool TryParseNetflix(Uri uri, string host, out string contentId)
    {
        contentId = string.Empty;

        if (host is not ("netflix.com" or "www.netflix.com"))
            return false;

        var segments = uri.AbsolutePath.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        var offset = segments.Length > 0 && NetflixLocalePattern.IsMatch(segments[0]) ? 1 : 0;

        if (segments.Length != offset + 2 ||
            segments[offset] != "watch" ||
            !NetflixIdPattern.IsMatch(segments[offset + 1]))
            return false;

        contentId = segments[offset + 1];
        return true;
    }

    private static string? GetQueryValue(string query, string key)
    {
        foreach (var pair in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            if (parts[0] == key && parts.Length == 2)
                return Uri.UnescapeDataString(parts[1]);
        }
        return null;
    }
}

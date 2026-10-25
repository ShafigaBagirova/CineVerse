using Domain.Enums;
using System.Text.RegularExpressions;

namespace Application.WatchRooms;

public static class WatchContentUrlParser
{
    private static readonly Regex YouTubeIdPattern = new("^[A-Za-z0-9_-]{11}$", RegexOptions.Compiled);

    public static bool TryParse(string? url, out WatchPlatform platform, out string contentId)
    {
        platform = default;
        contentId = string.Empty;

        if (string.IsNullOrWhiteSpace(url) ||
            !Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
            return false;

        var host = uri.Host.ToLowerInvariant();
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

        platform = WatchPlatform.YouTube;
        contentId = candidate;
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

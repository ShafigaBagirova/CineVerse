using System.Text.Json.Serialization;

namespace Infrastructure.Tmdb;

public sealed class TmdbVideoListResponse
{
    [JsonPropertyName("results")]
    public List<TmdbVideoItem> Results { get; set; } = new();
}
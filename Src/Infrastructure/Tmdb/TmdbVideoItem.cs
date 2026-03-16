using System.Text.Json.Serialization;

namespace Infrastructure.Tmdb;

public sealed class TmdbVideoItem
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = null!;

    [JsonPropertyName("site")]
    public string Site { get; set; } = null!;

    [JsonPropertyName("type")]
    public string Type { get; set; } = null!;

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("official")]
    public bool Official { get; set; }
}
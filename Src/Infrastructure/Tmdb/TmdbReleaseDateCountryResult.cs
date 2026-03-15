using System.Text.Json.Serialization;

namespace Infrastructure.Tmdb;

public sealed class TmdbReleaseDateCountryResult
{
    [JsonPropertyName("iso_3166_1")]
    public string? Iso31661 { get; set; }

    [JsonPropertyName("release_dates")]
    public List<TmdbReleaseDateItem> ReleaseDates { get; set; } = new();
}
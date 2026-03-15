using System.Text.Json.Serialization;

namespace Infrastructure.Tmdb;

public sealed class TmdbReleaseDatesWrapper
{
    [JsonPropertyName("results")]
    public List<TmdbReleaseDateCountryResult> Results { get; set; } = new();
}

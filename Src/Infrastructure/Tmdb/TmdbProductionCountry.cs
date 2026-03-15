using System.Text.Json.Serialization;

namespace Infrastructure.Tmdb;

public sealed class TmdbProductionCountry
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

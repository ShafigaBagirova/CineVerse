using System.Text.Json.Serialization;

namespace Infrastructure.Tmdb;


public sealed class TmdbReleaseDateItem
{
    [JsonPropertyName("certification")]
    public string? Certification { get; set; }
}
using System.Text.Json.Serialization;

namespace Infrastructure.Tmdb;

public sealed class TmdbCrewMember
{
    [JsonPropertyName("job")]
    public string? Job { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
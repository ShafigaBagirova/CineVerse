using System.Text.Json.Serialization;

namespace Infrastructure.Tmdb;

public class TmdbGenreItem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;
}

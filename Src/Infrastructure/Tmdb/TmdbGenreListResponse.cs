using System.Text.Json.Serialization;

namespace Infrastructure.Tmdb;

public class TmdbGenreListResponse
{
    [JsonPropertyName("genres")]
    public List<TmdbGenreItem> Genres { get; set; } = new();
}
using System.Text.Json.Serialization;

namespace Infrastructure.Tmdb;

public sealed class TmdbMovieListResponse
{
    [JsonPropertyName("results")]
    public List<TmdbMovieListItem> Results { get; set; } = new();
}
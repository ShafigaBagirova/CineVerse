using System.Text.Json.Serialization;

namespace Application.Movies.Dtos;

public class TmdbMovieDetailsDto
{
    [JsonPropertyName("runtime")]
    public int? Runtime { get; set; }
}
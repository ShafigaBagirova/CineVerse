using System.Text.Json.Serialization;
using System.Linq;

namespace Application.Movies.Dtos;

public class TmdbMovieDetailsDto
{
    [JsonPropertyName("runtime")]
    public int? Runtime { get; set; }

    [JsonPropertyName("credits")]
    public TmdbMovieCreditsDto? Credits { get; set; }

    [JsonIgnore]
    public string? Director =>
        Credits?.Crew?
            .FirstOrDefault(x => string.Equals(x.Job, "Director", StringComparison.OrdinalIgnoreCase))
            ?.Name;

    [JsonIgnore]
    public List<string> Cast =>
        (Credits?.Cast ?? Enumerable.Empty<TmdbMovieCastMemberDto>())
            .OrderBy(x => x.Order ?? int.MaxValue)
            .ThenBy(x => x.Name)
            .Select(x => x.Name?.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .Cast<string>()
            .ToList();
}

public class TmdbMovieCreditsDto
{
    [JsonPropertyName("cast")]
    public List<TmdbMovieCastMemberDto> Cast { get; set; } = new();

    [JsonPropertyName("crew")]
    public List<TmdbMovieCrewMemberDto> Crew { get; set; } = new();
}

public class TmdbMovieCastMemberDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("order")]
    public int? Order { get; set; }
}

public class TmdbMovieCrewMemberDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("job")]
    public string? Job { get; set; }
}
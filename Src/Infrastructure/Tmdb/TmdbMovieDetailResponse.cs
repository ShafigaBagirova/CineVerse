using System.Text.Json.Serialization;

namespace Infrastructure.Tmdb;

public sealed class TmdbMovieDetailResponse
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("overview")]
    public string Overview { get; set; } = string.Empty;

    [JsonPropertyName("tagline")]
    public string? Tagline { get; set; }

    [JsonPropertyName("release_date")]
    public string? ReleaseDate { get; set; }

    [JsonPropertyName("runtime")]
    public int? Runtime { get; set; }

    [JsonPropertyName("original_language")]
    public string? OriginalLanguage { get; set; }

    [JsonPropertyName("vote_average")]
    public decimal? VoteAverage { get; set; }

    [JsonPropertyName("production_countries")]
    public List<TmdbProductionCountry> ProductionCountries { get; set; } = new();

    [JsonPropertyName("credits")]
    public TmdbCredits? Credits { get; set; }

    [JsonPropertyName("release_dates")]
    public TmdbReleaseDatesWrapper? ReleaseDates { get; set; }
}
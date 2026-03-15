using System.Text.Json.Serialization;

namespace Infrastructure.Tmdb;

public sealed class TmdbCredits
{
    [JsonPropertyName("crew")]
    public List<TmdbCrewMember> Crew { get; set; } = new();
}
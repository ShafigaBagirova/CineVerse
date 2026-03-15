namespace Application.Common.Options;

public sealed class TmdbOptions
{
    public const string SectionName = "Tmdb";

    public string BaseUrl { get; set; } = "https://api.themoviedb.org/3/";
    public string ReadAccessToken { get; set; } = null!;
    public string ImageBaseUrl { get; set; } = "https://image.tmdb.org/t/p/original";
    public string Language { get; set; } = "en-US";
    public string Region { get; set; } = "US";
}

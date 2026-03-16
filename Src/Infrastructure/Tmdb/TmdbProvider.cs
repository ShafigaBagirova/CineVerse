using Application.Common.Interfaces;
using Application.Common.Options;
using Application.Movies.Dtos;
using Application.MovieVideos.Dtos;
using AutoMapper;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Infrastructure.Tmdb;

public sealed class TmdbMovieProvider : IMovieProvider
{
    private readonly HttpClient _httpClient;
    private readonly IMapper _mapper;
    private readonly TmdbOptions _options;

    public TmdbMovieProvider(
        HttpClient httpClient,
        IMapper mapper,
        IOptions<TmdbOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _mapper = mapper;

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ReadAccessToken);
    }

    public Task<IReadOnlyList<ExternalMovieDto>> GetNowPlayingAsync(CancellationToken cancellationToken = default)
        => GetMoviesAsync("movie/now_playing", cancellationToken);

    public Task<IReadOnlyList<ExternalMovieDto>> GetUpcomingAsync(CancellationToken cancellationToken = default)
        => GetMoviesAsync("movie/upcoming", cancellationToken);

    private async Task<IReadOnlyList<ExternalMovieDto>> GetMoviesAsync(
        string endpoint,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetFromJsonAsync<TmdbMovieListResponse>(
            $"{endpoint}?language={_options.Language}&region={_options.Region}&page=1",
            cancellationToken);

        if (response is null || response.Results.Count == 0)
            return [];

        return _mapper.Map<List<ExternalMovieDto>>(response.Results);
    }

    public async Task<List<ExternalMovieDto>> GetMoviesAsync(
        int page,
        CancellationToken cancellationToken = default)
    {
        var url = $"{_options.BaseUrl}/movie/popular?page={page}";

        using var response = await _httpClient.GetAsync(url, cancellationToken);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        var tmdbResponse = JsonSerializer.Deserialize<TmdbMovieListResponse>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (tmdbResponse is null)
            return new List<ExternalMovieDto>();

        return _mapper.Map<List<ExternalMovieDto>>(tmdbResponse.Results);
    }

    public async Task<ExternalTrailerDto?> GetTrailerAsync(
        long tmdbId,
        CancellationToken cancellationToken = default)
    {
        var url = $"{_options.BaseUrl}/movie/{tmdbId}/videos";

        using var response = await _httpClient.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        var tmdbResponse = JsonSerializer.Deserialize<TmdbVideoListResponse>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (tmdbResponse is null)
            return null;

        var trailer = tmdbResponse.Results
            .Where(x => x.Site == "YouTube" && x.Type == "Trailer")
            .OrderByDescending(x => x.Official)
            .FirstOrDefault();

        if (trailer is null)
            return null;

        return new ExternalTrailerDto
        {
            Key = trailer.Key,
            Site = trailer.Site,
            Type = trailer.Type,
            Name = trailer.Name,
            IsOfficial = trailer.Official
        };
    }
}
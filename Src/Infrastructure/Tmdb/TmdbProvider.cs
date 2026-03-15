using Application.Common.Interfaces;
using Application.Common.Options;
using Application.Movies.Dtos;
using AutoMapper;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

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
        _mapper = mapper;
        _options = options.Value;
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
}
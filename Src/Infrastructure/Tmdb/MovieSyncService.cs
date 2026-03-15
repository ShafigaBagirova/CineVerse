using Application.Common.Interfaces;
using Application.Movies.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Infrastructure.Tmdb;

public sealed class MovieSyncService : IMovieSyncService
{
    private readonly IMovieProvider _movieProvider;
    private readonly IMovieRepository _movieRepository;
    private readonly IMapper _mapper;

    public MovieSyncService(
        IMovieProvider movieProvider,
        IMovieRepository movieRepository,
        IMapper mapper)
    {
        _movieProvider = movieProvider;
        _movieRepository = movieRepository;
        _mapper = mapper;
    }

    public async Task SyncNowPlayingAsync(CancellationToken cancellationToken = default)
    {
        var externalMovies = await _movieProvider.GetNowPlayingAsync(cancellationToken);
        await SyncMoviesAsync(externalMovies, cancellationToken);
    }

    public async Task SyncUpcomingAsync(CancellationToken cancellationToken = default)
    {
        var externalMovies = await _movieProvider.GetUpcomingAsync(cancellationToken);
        await SyncMoviesAsync(externalMovies, cancellationToken);
    }

    private async Task SyncMoviesAsync(
        IReadOnlyList<ExternalMovieDto> externalMovies,
        CancellationToken cancellationToken)
    {
        foreach (var externalMovie in externalMovies)
        {
            var existingMovie = await _movieRepository
                .GetByTmdbIdAsync(externalMovie.ExternalId, cancellationToken);

            if (existingMovie is null)
            {
                var movie = _mapper.Map<Movie>(externalMovie);
                await _movieRepository.AddAsync(movie, cancellationToken);
            }
            else
            {
                _mapper.Map(externalMovie, existingMovie);
                await _movieRepository.UpdateAsync(existingMovie, cancellationToken);
            }
        }
    }
}
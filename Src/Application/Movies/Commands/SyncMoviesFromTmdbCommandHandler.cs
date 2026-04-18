using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Movies.Dtos;
using Application.Movies.Events;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Movies.Commands;

public sealed class SyncMoviesFromTmdbCommandHandler
    : IRequestHandler<SyncMoviesFromTmdbCommand, BaseResponse>
{
    private readonly IMovieProvider _movieProvider;
    private readonly IMovieRepository _movieRepository;
    private readonly IMovieVideoRepository _movieVideoRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<SyncMoviesFromTmdbCommandHandler> _logger;
    private readonly IGenreRepository _genreRepository;
    private readonly IMovieGenreRepository _movieGenreRepository;
    private readonly IPublisher _publisher;


    public SyncMoviesFromTmdbCommandHandler(
        IMovieProvider movieProvider,
        IMovieRepository movieRepository,
        IMovieVideoRepository movieVideoRepository,
        IMapper mapper,
        ILogger<SyncMoviesFromTmdbCommandHandler> logger,
        IGenreRepository genreRepository,
        IMovieGenreRepository movieGenreRepository,
        IPublisher publisher)
    {
        _movieProvider = movieProvider;
        _movieRepository = movieRepository;
        _movieVideoRepository = movieVideoRepository;
        _mapper = mapper;
        _logger = logger;
        _genreRepository = genreRepository;
        _movieGenreRepository = movieGenreRepository;
        _publisher = publisher;
    }

    public async Task<BaseResponse> Handle(
        SyncMoviesFromTmdbCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("TMDB movie sync started. Page: {Page}", request.Page);

        var externalMovies = await _movieProvider.GetMoviesAsync(request.Page, cancellationToken);

        var createdCount = 0;

        foreach (var externalMovie in externalMovies)
        {
            var details = await _movieProvider.GetMovieDetailsAsync( externalMovie.ExternalId,cancellationToken);

            externalMovie.DurationMinutes = details?.Runtime;
            var existingMovie = await _movieRepository
                .GetByTmdbIdAsync(externalMovie.ExternalId, cancellationToken);

            if (existingMovie is null)
            {
                _logger.LogInformation("Creating movie from TMDB Id {TmdbId}", externalMovie.ExternalId);

                var movie = _mapper.Map<Movie>(externalMovie);

                movie.Slug = await GenerateUniqueSlugAsync(
                    string.IsNullOrWhiteSpace(externalMovie.Slug)
                        ? externalMovie.Title
                        : externalMovie.Slug,
                    cancellationToken);

                movie.RatingCount = 0;

                await _movieRepository.AddAsync(movie, cancellationToken);
                await _movieRepository.SaveChangesAsync(cancellationToken);

                await SyncGenresAsync(movie.Id, externalMovie, cancellationToken);
                await SyncTrailerAsync(movie.Id, externalMovie.ExternalId, cancellationToken);

                createdCount++;
            }
            else
            {
                _logger.LogInformation("Updating movie from TMDB Id {TmdbId}", externalMovie.ExternalId);

                _mapper.Map(externalMovie, existingMovie);

                existingMovie.Slug = await GenerateUniqueSlugForUpdateAsync(
                    existingMovie.Id,
                    string.IsNullOrWhiteSpace(externalMovie.Slug)
                        ? externalMovie.Title
                        : externalMovie.Slug,
                    cancellationToken);

                await _movieRepository.UpdateAsync(existingMovie, cancellationToken);
                await _movieRepository.SaveChangesAsync(cancellationToken);

                await SyncGenresAsync(existingMovie.Id, externalMovie, cancellationToken);
                await SyncTrailerAsync(existingMovie.Id, externalMovie.ExternalId, cancellationToken);
            }
        }

        if (createdCount > 0)
        {
            await _publisher.Publish(
                new MoviesBulkImportedEvent(createdCount),
                cancellationToken);

            _logger.LogInformation(
                "Bulk movie notification published. CreatedCount: {CreatedCount}",
                createdCount);
        }

        _logger.LogInformation(
            "TMDB movie sync completed successfully. CreatedCount: {CreatedCount}",
            createdCount);

        return new BaseResponse
        {
            Success = true,
            Message = "Movies synced succesfully from TMDB."
        };
    }

    private async Task SyncGenresAsync(
        int movieId,
        ExternalMovieDto externalMovie,
        CancellationToken cancellationToken)
    {
        if (externalMovie.GenreIds is null || !externalMovie.GenreIds.Any())
            return;

        var existingMovieGenres = await _movieGenreRepository
            .GetByMovieIdAsync(movieId, cancellationToken);

        var existingGenreIds = existingMovieGenres
            .Select(x => x.GenreId)
            .ToHashSet();

        var newGenreIds = new HashSet<int>();

        foreach (var tmdbGenreId in externalMovie.GenreIds)
        {
            var genre = await _genreRepository
                .GetByTmdbGenreIdAsync(tmdbGenreId, cancellationToken);

            if (genre is null)
            {
                _logger.LogWarning(
                    "Genre with TMDB GenreId {TmdbGenreId} not found while syncing movie {MovieId}",
                    tmdbGenreId,
                    movieId);

                continue;
            }

            newGenreIds.Add(genre.Id);

            if (!existingGenreIds.Contains(genre.Id))
            {
                var movieGenre = new MovieGenre
                {
                    MovieId = movieId,
                    GenreId = genre.Id,
                    IsPrimary = false,
                    Order = 0
                };

                await _movieGenreRepository.AddAsync(movieGenre, cancellationToken);
            }
        }

        var movieGenresToRemove = existingMovieGenres
            .Where(x => !newGenreIds.Contains(x.GenreId))
            .ToList();

        if (movieGenresToRemove.Any())
        {
            await _movieGenreRepository.RemoveRangeAsync(movieGenresToRemove, cancellationToken);
        }

        await _movieGenreRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Genres synced for MovieId {MovieId}. GenreCount: {GenreCount}",
            movieId,
            newGenreIds.Count);
    }

    private async Task SyncTrailerAsync(
        int movieId,
        long tmdbId,
        CancellationToken cancellationToken)
    {
        var trailer = await _movieProvider.GetTrailerAsync(tmdbId, cancellationToken);

        if (trailer is null)
            return;

        var existingTrailer = await _movieVideoRepository
            .GetTrailerByMovieIdAsync(movieId, cancellationToken);

        if (existingTrailer is null)
        {
            var movieVideo = new MovieVideo
            {
                MovieId = movieId,
                VideoKey = trailer.Key,
                Site = trailer.Site,
                Type = trailer.Type,
                Name = trailer.Name
            };

            await _movieVideoRepository.AddAsync(movieVideo, cancellationToken);
            await _movieVideoRepository.SaveChangesAsync(cancellationToken);
        }
        else
        {
            existingTrailer.VideoKey = trailer.Key;
            existingTrailer.Site = trailer.Site;
            existingTrailer.Type = trailer.Type;
            existingTrailer.Name = trailer.Name;

            await _movieVideoRepository.UpdateAsync(existingTrailer, cancellationToken);
            await _movieVideoRepository.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<string> GenerateUniqueSlugAsync(
        string titleOrSlug,
        CancellationToken cancellationToken)
    {
        var baseSlug = SlugHelper.Generate(titleOrSlug);

        if (string.IsNullOrWhiteSpace(baseSlug))
            baseSlug = "movie";

        var slug = baseSlug;
        var counter = 1;

        while (await _movieRepository.ExistsBySlugAsync(slug, cancellationToken))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }

    private async Task<string> GenerateUniqueSlugForUpdateAsync(
        int movieId,
        string titleOrSlug,
        CancellationToken cancellationToken)
    {
        var baseSlug = SlugHelper.Generate(titleOrSlug);

        if (string.IsNullOrWhiteSpace(baseSlug))
            baseSlug = "movie";

        var slug = baseSlug;
        var counter = 1;

        while (await _movieRepository.ExistsBySlugAsync(slug, cancellationToken))
        {
            var existing = await _movieRepository.GetBySlugAsync(slug, cancellationToken);

            if (existing is not null && existing.Id == movieId)
                return slug;

            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        return slug;
    }
}
using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
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

    public SyncMoviesFromTmdbCommandHandler(
        IMovieProvider movieProvider,
        IMovieRepository movieRepository,
        IMovieVideoRepository movieVideoRepository,
        IMapper mapper,
        ILogger<SyncMoviesFromTmdbCommandHandler> logger)
    {
        _movieProvider = movieProvider;
        _movieRepository = movieRepository;
        _movieVideoRepository = movieVideoRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(
        SyncMoviesFromTmdbCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("TMDB movie sync started. Page: {Page}", request.Page);

        var externalMovies = await _movieProvider.GetMoviesAsync(request.Page, cancellationToken);

        foreach (var externalMovie in externalMovies)
        {
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
                movie.MediaItems = new List<MoviePoster>();

                await _movieRepository.AddAsync(movie, cancellationToken);
                await _movieRepository.SaveChangesAsync(cancellationToken);

                await SyncTrailerAsync(movie.Id, externalMovie.ExternalId, cancellationToken);
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

                await SyncTrailerAsync(existingMovie.Id, externalMovie.ExternalId, cancellationToken);
            }
        }

        _logger.LogInformation("TMDB movie sync completed successfully.");

        return new BaseResponse
        {
            Success = true,
            Message = "Movies synced succesfully from TMDB."
        };
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
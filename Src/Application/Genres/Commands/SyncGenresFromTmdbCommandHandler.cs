using Application.Common.Interfaces;
using Application.Common.Responses;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Genres.Commands;

public sealed class SyncGenresFromTmdbCommandHandler
    : IRequestHandler<SyncGenresFromTmdbCommand, BaseResponse>
{
    private readonly IMovieProvider _movieProvider;
    private readonly IGenreRepository _genreRepository;
    private readonly ILogger<SyncGenresFromTmdbCommandHandler> _logger;

    public SyncGenresFromTmdbCommandHandler(
        IMovieProvider movieProvider,
        IGenreRepository genreRepository,
        ILogger<SyncGenresFromTmdbCommandHandler> logger)
    {
        _movieProvider = movieProvider;
        _genreRepository = genreRepository;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(
        SyncGenresFromTmdbCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("TMDB genre sync started.");

        var externalGenres = await _movieProvider.GetGenresAsync(cancellationToken);

        if (externalGenres is null || externalGenres.Count == 0)
        {
            _logger.LogWarning("TMDB genre sync returned no genres.");
            return BaseResponse.Fail("No genres were returned from TMDB.");
        }

        int createdCount = 0;
        int updatedCount = 0;

        foreach (var externalGenre in externalGenres)
        {
            _logger.LogInformation(
                "Processing TMDB genre. TmdbGenreId: {TmdbGenreId}, Name: {GenreName}",
                externalGenre.Id,
                externalGenre.Name);

            var existingGenre = await _genreRepository
                .GetByTmdbGenreIdAsync(externalGenre.Id, cancellationToken);

            if (existingGenre is null)
            {
                var genre = new Genre
                {
                    TmdbGenreId = externalGenre.Id,
                    Name = externalGenre.Name
                };

                await _genreRepository.AddAsync(genre, cancellationToken);
                createdCount++;

                _logger.LogInformation(
                    "Genre created. TmdbGenreId: {TmdbGenreId}, Name: {GenreName}",
                    externalGenre.Id,
                    externalGenre.Name);
            }
            else
            {
                if (!string.Equals(existingGenre.Name, externalGenre.Name, StringComparison.Ordinal))
                {
                    existingGenre.Name = externalGenre.Name;
                    await _genreRepository.UpdateAsync(existingGenre, cancellationToken);
                    updatedCount++;

                    _logger.LogInformation(
                        "Genre updated. GenreId: {GenreId}, TmdbGenreId: {TmdbGenreId}, NewName: {GenreName}",
                        existingGenre.Id,
                        existingGenre.TmdbGenreId,
                        existingGenre.Name);
                }
                else
                {
                    _logger.LogInformation(
                        "Genre already up to date. GenreId: {GenreId}, TmdbGenreId: {TmdbGenreId}",
                        existingGenre.Id,
                        existingGenre.TmdbGenreId);
                }
            }
        }

        await _genreRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "TMDB genre sync completed successfully. CreatedCount: {CreatedCount}, UpdatedCount: {UpdatedCount}, TotalFetched: {TotalFetched}",
            createdCount,
            updatedCount,
            externalGenres.Count);

        return BaseResponse.Ok(
            $"Genres synced successfully. Created: {createdCount}, Updated: {updatedCount}.");
    }
}
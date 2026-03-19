using Application.Common.Interfaces;
using Application.Common.Responses;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Movies.Commands;

public sealed class CreateMovieGenreCommandHandler
    : IRequestHandler<CreateMovieGenreCommand, BaseResponse>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IGenreRepository _genreRepository;
    private readonly IMovieGenreRepository _movieGenreRepository;
    private readonly ILogger<CreateMovieGenreCommandHandler> _logger;

    public CreateMovieGenreCommandHandler(
        IMovieRepository movieRepository,
        IGenreRepository genreRepository,
        IMovieGenreRepository movieGenreRepository,
        ILogger<CreateMovieGenreCommandHandler> logger)
    {
        _movieRepository = movieRepository;
        _genreRepository = genreRepository;
        _movieGenreRepository = movieGenreRepository;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(
        CreateMovieGenreCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "CreateMovieGenre started. MovieId: {MovieId}, GenreId: {GenreId}, IsPrimary: {IsPrimary}, Order: {Order}",
            request.MovieId,
            request.Request.GenreId,
            request.Request.IsPrimary,
            request.Request.Order);

        var movie = await _movieRepository.GetByIdAsync(request.MovieId, cancellationToken);

        if (movie is null)
        {
            _logger.LogWarning(
                "CreateMovieGenre failed. Movie not found. MovieId: {MovieId}",
                request.MovieId);

            throw new KeyNotFoundException("Movie could not be found.");
        }

        var genre = await _genreRepository.GetByIdAsync(request.Request.GenreId, cancellationToken);

        if (genre is null)
        {
            _logger.LogWarning(
                "CreateMovieGenre failed. Genre not found. GenreId: {GenreId}",
                request.Request.GenreId);

            throw new KeyNotFoundException("Genre could not be found.");
        }

        var exists = await _movieGenreRepository.ExistsAsync(
            request.MovieId,
            request.Request.GenreId,
            cancellationToken);

        if (exists)
        {
            _logger.LogWarning(
                "CreateMovieGenre failed. Relation already exists. MovieId: {MovieId}, GenreId: {GenreId}",
                request.MovieId,
                request.Request.GenreId);

            return BaseResponse.Fail("This genre is already assigned to the movie.");
        }

        if (request.Request.IsPrimary)
        {
            var currentPrimary = await _movieGenreRepository
                .GetPrimaryByMovieIdAsync(request.MovieId, cancellationToken);

            if (currentPrimary is not null)
            {
                _logger.LogWarning(
                    "CreateMovieGenre failed. Primary genre already exists. MovieId: {MovieId}, ExistingPrimaryGenreId: {GenreId}",
                    request.MovieId,
                    currentPrimary.GenreId);

                return BaseResponse.Fail("This movie already has a primary genre.");
            }
        }

        var movieGenre = new MovieGenre
        {
            MovieId = request.MovieId,
            GenreId = request.Request.GenreId,
            IsPrimary = request.Request.IsPrimary,
            Order = request.Request.Order
        };

        await _movieGenreRepository.AddAsync(movieGenre, cancellationToken);
        await _movieGenreRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "CreateMovieGenre completed successfully. MovieId: {MovieId}, GenreId: {GenreId}",
            request.MovieId,
            request.Request.GenreId);

        return BaseResponse.Ok("Genre assigned to movie successfully.");
    }
}
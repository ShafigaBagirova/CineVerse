using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Movies.Commands;

public sealed class UpdateMovieGenreCommandHandler
    : IRequestHandler<UpdateMovieGenreCommand, BaseResponse>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IMovieGenreRepository _movieGenreRepository;
    private readonly ILogger<UpdateMovieGenreCommandHandler> _logger;

    public UpdateMovieGenreCommandHandler(
        IMovieRepository movieRepository,
        IMovieGenreRepository movieGenreRepository,
        ILogger<UpdateMovieGenreCommandHandler> logger)
    {
        _movieRepository = movieRepository;
        _movieGenreRepository = movieGenreRepository;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(
        UpdateMovieGenreCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "UpdateMovieGenre started. MovieId: {MovieId}, GenreId: {GenreId}, IsPrimary: {IsPrimary}, Order: {Order}",
            request.MovieId,
            request.GenreId,
            request.Request.IsPrimary,
            request.Request.Order);

        var movie = await _movieRepository.GetByIdAsync(request.MovieId, cancellationToken);

        if (movie is null)
        {
            _logger.LogWarning(
                "UpdateMovieGenre failed. Movie not found. MovieId: {MovieId}",
                request.MovieId);

            throw new KeyNotFoundException("Movie could not be found.");
        }

        var movieGenre = await _movieGenreRepository.GetByIdsAsync(
            request.MovieId,
            request.GenreId,
            cancellationToken);

        if (movieGenre is null)
        {
            _logger.LogWarning(
                "UpdateMovieGenre failed. Relation not found. MovieId: {MovieId}, GenreId: {GenreId}",
                request.MovieId,
                request.GenreId);

            throw new KeyNotFoundException("Movie genre relation could not be found.");
        }

        if (request.Request.IsPrimary)
        {
            var currentPrimary = await _movieGenreRepository
                .GetPrimaryByMovieIdAsync(request.MovieId, cancellationToken);

            if (currentPrimary is not null && currentPrimary.GenreId != request.GenreId)
            {
                _logger.LogWarning(
                    "UpdateMovieGenre failed. Another primary genre already exists. MovieId: {MovieId}, ExistingPrimaryGenreId: {GenreId}",
                    request.MovieId,
                    currentPrimary.GenreId);

                return BaseResponse.Fail("This movie already has another primary genre.");
            }
        }

        movieGenre.IsPrimary = request.Request.IsPrimary;
        movieGenre.Order = request.Request.Order;

        await _movieGenreRepository.UpdateAsync(movieGenre, cancellationToken);
        await _movieGenreRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "UpdateMovieGenre completed successfully. MovieId: {MovieId}, GenreId: {GenreId}",
            request.MovieId,
            request.GenreId);

        return BaseResponse.Ok("Movie genre updated successfully.");
    }
}
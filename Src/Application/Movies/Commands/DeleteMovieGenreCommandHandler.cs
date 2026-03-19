using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Movies.Commands;


public sealed class DeleteMovieGenreCommandHandler
    : IRequestHandler<DeleteMovieGenreCommand, BaseResponse>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IMovieGenreRepository _movieGenreRepository;
    private readonly ILogger<DeleteMovieGenreCommandHandler> _logger;

    public DeleteMovieGenreCommandHandler(
        IMovieRepository movieRepository,
        IMovieGenreRepository movieGenreRepository,
        ILogger<DeleteMovieGenreCommandHandler> logger)
    {
        _movieRepository = movieRepository;
        _movieGenreRepository = movieGenreRepository;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(
        DeleteMovieGenreCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "DeleteMovieGenre started. MovieId: {MovieId}, GenreId: {GenreId}",
            request.MovieId,
            request.GenreId);

        var movie = await _movieRepository.GetByIdAsync(request.MovieId, cancellationToken);

        if (movie is null)
        {
            _logger.LogWarning(
                "DeleteMovieGenre failed. Movie not found. MovieId: {MovieId}",
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
                "DeleteMovieGenre failed. MovieGenre relation not found. MovieId: {MovieId}, GenreId: {GenreId}",
                request.MovieId,
                request.GenreId);

            throw new KeyNotFoundException("Movie genre relation could not be found.");
        }

        await _movieGenreRepository.DeleteAsync(movieGenre, cancellationToken);
        await _movieGenreRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "DeleteMovieGenre completed successfully. MovieId: {MovieId}, GenreId: {GenreId}",
            request.MovieId,
            request.GenreId);

        return BaseResponse.Ok("Movie genre deleted successfully.");
    }
}
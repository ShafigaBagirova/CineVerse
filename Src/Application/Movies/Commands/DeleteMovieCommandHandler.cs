using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Movies.Commands;

public sealed class DeleteMovieCommandHandler
    : IRequestHandler<DeleteMovieCommand, BaseResponse>
{
    private readonly IMovieRepository _movieRepository;
    private readonly ILogger<DeleteMovieCommandHandler> _logger;

    public DeleteMovieCommandHandler(
        IMovieRepository movieRepository,
        ILogger<DeleteMovieCommandHandler> logger)
    {
        _movieRepository = movieRepository;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(DeleteMovieCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("DeleteMovieCommand started for MovieId {MovieId}", request.Id);

        var movie = await _movieRepository.GetByIdAsync(request.Id, cancellationToken);

        if (movie is null)
        {
            _logger.LogWarning("DeleteMovieCommand failed. Movie with Id {MovieId} not found.", request.Id);

            return new BaseResponse
            {
                Success = false,
                Message = "Movie could not be found."
            };
        }

        await _movieRepository.DeleteAsync(movie, cancellationToken);
        await _movieRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Movie with Id {MovieId} deleted successfully.", request.Id);

        return new BaseResponse
        {
            Success = true,
            Message = "Movie deleted successfully."
        };
    }
}

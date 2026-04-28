using Application.Common.Helpers;
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
    private readonly ICacheService _cacheService;

    public DeleteMovieCommandHandler(
        IMovieRepository movieRepository,
        ILogger<DeleteMovieCommandHandler> logger,
        ICacheService cacheService)
    {
        _movieRepository = movieRepository;
        _logger = logger;
        _cacheService = cacheService;
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

        await _cacheService.RemoveAsync($"{CacheKeys.MovieByIdPrefix}{request.Id}", cancellationToken);
        await _cacheService.RemoveByPrefixAsync(CacheKeys.MoviesPagedPrefix);
        await _cacheService.RemoveByPrefixAsync(CacheKeys.MovieBySlugPrefix);

        _logger.LogInformation(
            "Movie with Id {MovieId} deleted successfully. Movie detail and movies list caches invalidated.",
            request.Id);

        return new BaseResponse
        {
            Success = true,
            Message = "Movie deleted successfully."
        };
    }
}
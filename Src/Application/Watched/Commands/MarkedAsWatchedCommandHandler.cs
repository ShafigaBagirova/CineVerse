using Application.Common.Interfaces;
using Application.Common.Responses;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Watched.Commands;

public sealed class MarkAsWatchedCommandHandler
    : IRequestHandler<MarkAsWatchedCommand, BaseResponse>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IWatchLogRepository _watchLogRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<MarkAsWatchedCommandHandler> _logger;

    public MarkAsWatchedCommandHandler(
        IMovieRepository movieRepository,
        IWatchLogRepository watchLogRepository,
        ICurrentUserService currentUserService,
        ILogger<MarkAsWatchedCommandHandler> logger)
    {
        _movieRepository = movieRepository;
        _watchLogRepository = watchLogRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(
        MarkAsWatchedCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "MarkAsWatched started. MovieId: {MovieId}",
            request.MovieId);

        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning(
                "MarkAsWatched failed. Authenticated user not found. MovieId: {MovieId}",
                request.MovieId);

            return BaseResponse.Fail("Authenticated user could not be found.");
        }

        var movie = await _movieRepository.GetByIdAsync(request.MovieId, cancellationToken);

        if (movie is null)
        {
            _logger.LogWarning(
                "MarkAsWatched failed. Movie not found. MovieId: {MovieId}",
                request.MovieId);

            return BaseResponse.Fail("Movie could not be found.");
        }

        var exists = await _watchLogRepository.ExistsAsync(
            request.MovieId,
            userId,
            cancellationToken);

        if (exists)
        {
            _logger.LogWarning(
                "MarkAsWatched failed. Movie already marked as watched. MovieId: {MovieId}, UserId: {UserId}",
                request.MovieId,
                userId);

            return BaseResponse.Fail("This movie is already marked as watched.");
        }

        var watchLog = new WatchLog
        {
            MovieId = request.MovieId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _watchLogRepository.AddAsync(watchLog, cancellationToken);
        await _watchLogRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "MarkAsWatched completed successfully. WatchLogId: {WatchLogId}, MovieId: {MovieId}, UserId: {UserId}",
            watchLog.Id,
            watchLog.MovieId,
            watchLog.UserId);

        return BaseResponse.Ok("Movie marked as watched successfully.");
    }
}

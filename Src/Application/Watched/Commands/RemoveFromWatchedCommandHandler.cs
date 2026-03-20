using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Watched.Commands;

public sealed class RemoveFromWatchedCommandHandler
    : IRequestHandler<RemoveFromWatchedCommand, BaseResponse>
{
    private readonly IWatchLogRepository _watchLogRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<RemoveFromWatchedCommandHandler> _logger;

    public RemoveFromWatchedCommandHandler(
        IWatchLogRepository watchLogRepository,
        ICurrentUserService currentUserService,
        ILogger<RemoveFromWatchedCommandHandler> logger)
    {
        _watchLogRepository = watchLogRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(
        RemoveFromWatchedCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "RemoveFromWatched started. MovieId: {MovieId}",
            request.MovieId);

        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning(
                "RemoveFromWatched failed. Authenticated user not found. MovieId: {MovieId}",
                request.MovieId);

            return BaseResponse.Fail("Authenticated user could not be found.");
        }

        var watchLog = await _watchLogRepository.GetByMovieAndUserAsync(
            request.MovieId,
            userId,
            cancellationToken);

        if (watchLog is null)
        {
            _logger.LogWarning(
                "RemoveFromWatched failed. Watch log not found. MovieId: {MovieId}, UserId: {UserId}",
                request.MovieId,
                userId);

            return BaseResponse.Fail("This movie is not in your watched list.");
        }

        await _watchLogRepository.DeleteAsync(watchLog, cancellationToken);
        await _watchLogRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "RemoveFromWatched completed successfully. WatchLogId: {WatchLogId}, MovieId: {MovieId}, UserId: {UserId}",
            watchLog.Id,
            watchLog.MovieId,
            watchLog.UserId);

        return BaseResponse.Ok("Movie removed from watched list successfully.");
    }
}
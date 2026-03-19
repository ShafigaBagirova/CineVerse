using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.WatchListItems.Commands;

public sealed class RemoveFromWatchlistCommandHandler
    : IRequestHandler<RemoveFromWatchlistCommand, BaseResponse>
{
    private readonly IWatchListItemRepository _watchlistRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<RemoveFromWatchlistCommandHandler> _logger;

    public RemoveFromWatchlistCommandHandler(
        IWatchListItemRepository watchlistRepository,
        ICurrentUserService currentUserService,
        ILogger<RemoveFromWatchlistCommandHandler> logger)
    {
        _watchlistRepository = watchlistRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(
        RemoveFromWatchlistCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "RemoveFromWatchlist started. MovieId: {MovieId}",
            request.MovieId);

        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning(
                "RemoveFromWatchlist failed. Authenticated user not found. MovieId: {MovieId}",
                request.MovieId);

            return BaseResponse.Fail("Authenticated user could not be found.");
        }

        var watchlistItem = await _watchlistRepository.GetByMovieAndUserAsync(
            request.MovieId,
            userId,
            cancellationToken);

        if (watchlistItem is null)
        {
            _logger.LogWarning(
                "RemoveFromWatchlist failed. Watchlist item not found. MovieId: {MovieId}, UserId: {UserId}",
                request.MovieId,
                userId);

            return BaseResponse.Fail("This movie is not in your watchlist.");
        }

        await _watchlistRepository.DeleteAsync(watchlistItem, cancellationToken);
        await _watchlistRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "RemoveFromWatchlist completed successfully. WatchlistItemId: {WatchlistItemId}, MovieId: {MovieId}, UserId: {UserId}",
            watchlistItem.Id,
            watchlistItem.MovieId,
            watchlistItem.UserId);

        return BaseResponse.Ok("Movie removed from watchlist successfully.");
    }
}
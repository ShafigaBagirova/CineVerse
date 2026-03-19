using Application.Common.Interfaces;
using Application.Common.Responses;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.WatchListItems.Commands;

public sealed class AddToWatchlistCommandHandler
    : IRequestHandler<AddToWatchlistCommand, BaseResponse>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IWatchListItemRepository _watchlistRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AddToWatchlistCommandHandler> _logger;

    public AddToWatchlistCommandHandler(
        IMovieRepository movieRepository,
        IWatchListItemRepository watchlistRepository,
        ICurrentUserService currentUserService,
        ILogger<AddToWatchlistCommandHandler> logger)
    {
        _movieRepository = movieRepository;
        _watchlistRepository = watchlistRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(
        AddToWatchlistCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "AddToWatchlist started. MovieId: {MovieId}",
            request.MovieId);

        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning(
                "AddToWatchlist failed. Authenticated user not found. MovieId: {MovieId}",
                request.MovieId);

            return BaseResponse.Fail("Authenticated user could not be found.");
        }

        var movie = await _movieRepository.GetByIdAsync(request.MovieId, cancellationToken);

        if (movie is null)
        {
            _logger.LogWarning(
                "AddToWatchlist failed. Movie not found. MovieId: {MovieId}",
                request.MovieId);

            return BaseResponse.Fail("Movie could not be found.");
        }

        var exists = await _watchlistRepository.ExistsAsync(
            request.MovieId,
            userId,
            cancellationToken);

        if (exists)
        {
            _logger.LogWarning(
                "AddToWatchlist failed. Movie already exists in watchlist. MovieId: {MovieId}, UserId: {UserId}",
                request.MovieId,
                userId);

            return BaseResponse.Fail("This movie is already in your watchlist.");
        }

        var watchlistItem = new WatchListItem
        {
            MovieId = request.MovieId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _watchlistRepository.AddAsync(watchlistItem, cancellationToken);
        await _watchlistRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "AddToWatchlist completed successfully. WatchlistItemId: {WatchlistItemId}, MovieId: {MovieId}, UserId: {UserId}",
            watchlistItem.Id,
            watchlistItem.MovieId,
            watchlistItem.UserId);

        return BaseResponse.Ok("Movie added to watchlist successfully.");
    }
}
using Application.Common.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Movies.Events;

public sealed class MovieCreatedEventHandler
    : INotificationHandler<MovieCreatedEvent>
{
    private readonly IUserIdsProvider _userIdsProvider;
    private readonly IInAppNotificationService _inAppNotificationService;
    private readonly ILogger<MovieCreatedEventHandler> _logger;

    public MovieCreatedEventHandler(
        IUserIdsProvider userIdsProvider,
        IInAppNotificationService inAppNotificationService,
        ILogger<MovieCreatedEventHandler> logger)
    {
        _userIdsProvider = userIdsProvider;
        _inAppNotificationService = inAppNotificationService;
        _logger = logger;
    }

    public async Task Handle(MovieCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "MovieCreatedEventHandler started. MovieId: {MovieId}, Title: {Title}",
            notification.MovieId,
            notification.Title);

        var userIds = await _userIdsProvider.GetAllUserIdsAsync(cancellationToken);

        if (userIds.Count == 0)
        {
            _logger.LogInformation(
                "MovieCreatedEventHandler skipped. No users found. MovieId: {MovieId}",
                notification.MovieId);

            return;
        }

        var title = "New movie added";
        var message = $"A new movie \"{notification.Title}\" is now available in CineVerse.";

        await _inAppNotificationService.CreateForUsersAsync(
            userIds,
            title,
            message,
            InAppNotificationType.NewMovieAdded,
            cancellationToken);

        _logger.LogInformation(
            "MovieCreatedEventHandler completed successfully. MovieId: {MovieId}, UserCount: {UserCount}",
            notification.MovieId,
            userIds.Count);
    }
}
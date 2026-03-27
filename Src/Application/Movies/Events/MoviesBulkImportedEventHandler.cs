using Application.Common.Interfaces;
using Application.Notifications.Commands;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Movies.Events;

public sealed class MoviesBulkImportedEventHandler
    : INotificationHandler<MoviesBulkImportedEvent>
{
    private readonly IUserIdsProvider _userIdsProvider;
    private readonly IInAppNotificationService _inAppNotificationService;
    private readonly ISender _sender;
    private readonly ILogger<MoviesBulkImportedEventHandler> _logger;

    public MoviesBulkImportedEventHandler(
        IUserIdsProvider userIdsProvider,
        IInAppNotificationService inAppNotificationService,
        ISender sender,
        ILogger<MoviesBulkImportedEventHandler> logger)
    {
        _userIdsProvider = userIdsProvider;
        _inAppNotificationService = inAppNotificationService;
        _sender = sender;
        _logger = logger;
    }

    public async Task Handle(MoviesBulkImportedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "MoviesBulkImportedEventHandler started. Count: {Count}",
            notification.Count);

        if (notification.Count <= 0)
        {
            _logger.LogInformation("MoviesBulkImportedEventHandler skipped. Count is 0.");
            return;
        }

        var userIds = await _userIdsProvider.GetAllUserIdsAsync(cancellationToken);

        if (userIds.Count == 0)
        {
            _logger.LogInformation("MoviesBulkImportedEventHandler skipped. No users found.");
            return;
        }

        var title = notification.Count == 1 ? "New movie added" : "New movies added";
        var message = notification.Count == 1
            ? "A new movie is now available in CineVerse."
            : $"{notification.Count} new movies are now available in CineVerse.";

        await _inAppNotificationService.CreateForUsersAsync(
            userIds,
            title,
            message,
            InAppNotificationType.NewMovieAdded,
            cancellationToken);

        try
        {
            await _sender.Send(
                new NotifyAllUsersRecommendationsCommand(),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to trigger recommendation notifications after bulk import.");
        }

        _logger.LogInformation(
            "MoviesBulkImportedEventHandler completed successfully. Count: {Count}, UserCount: {UserCount}",
            notification.Count,
            userIds.Count);
    }
}
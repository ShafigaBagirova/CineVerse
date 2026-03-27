using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Notifications;

public class InAppNotificationService:IInAppNotificationService
{
    private readonly INotificationRepository _notificationRepository;

    public InAppNotificationService(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task CreateAsync(string userId, string title,string message,InAppNotificationType type,CancellationToken cancellationToken)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _notificationRepository.AddAsync(notification, cancellationToken);
        await _notificationRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task CreateForUsersAsync(List<string> userIds,string title,string message,InAppNotificationType type, CancellationToken cancellationToken)
    {
        var notifications = userIds
            .Distinct()
            .Select(userId => new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        if (notifications.Count == 0)
            return;

        await _notificationRepository.AddRangeAsync(notifications, cancellationToken);
        await _notificationRepository.SaveChangesAsync(cancellationToken);
    }
}

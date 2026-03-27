using Domain.Enums;

namespace Application.Common.Interfaces;

public interface IInAppNotificationService
{
    Task CreateAsync(string userId,string title,string message,InAppNotificationType type,CancellationToken cancellationToken);
    Task CreateForUsersAsync(List<string> userIds,string title,string message,InAppNotificationType type,CancellationToken cancellationToken);
}

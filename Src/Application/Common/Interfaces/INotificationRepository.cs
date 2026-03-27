using Domain.Entities;

namespace Application.Common.Interfaces;

public interface INotificationRepository:IRepository<Notification,int>
{
    Task AddRangeAsync(List<Notification> notifications, CancellationToken cancellationToken);
    Task<List<Notification>> GetByUserIdAsync(string userId,int pageNumber,int pageSize,CancellationToken cancellationToken);
    Task<int> CountByUserIdAsync(string userId, CancellationToken cancellationToken);
    Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken);
}

using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class NotificationRepository:GenericRepository<Notification,int>,INotificationRepository
{
    private readonly CineVerseDbContext _context;

    public NotificationRepository(CineVerseDbContext context):base(context)
    {
        _context = context;
    }
    public async Task AddRangeAsync(List<Notification> notifications, CancellationToken cancellationToken)
    {
        await _context.Notifications.AddRangeAsync(notifications, cancellationToken);
    }
    public async Task<List<Notification>> GetByUserIdAsync(
       string userId,
       int pageNumber,
       int pageSize,
       CancellationToken cancellationToken)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByUserIdAsync(string userId, CancellationToken cancellationToken)
    {
        return await _context.Notifications
            .CountAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken)
    {
        return await _context.Notifications
            .CountAsync(x => x.UserId == userId && !x.IsRead, cancellationToken);
    }
}

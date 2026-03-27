using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class RecommendationNotificationLogRepository:GenericRepository<RecommendationNotificationLog,int>, IRecommendationNotificationLogRepository
{
    private readonly CineVerseDbContext _context;
    public RecommendationNotificationLogRepository(CineVerseDbContext context) : base(context)
    {
        _context = context;
    }
    public async Task<bool> ExistsAsync(
        string userId,
        RecommendationTargetType targetType,
        string targetId,
        CancellationToken cancellationToken)
    {
        return await _context.RecommendationNotificationLogs
            .AsNoTracking()
            .AnyAsync(
                x => x.UserId == userId &&
                     x.TargetType == targetType &&
                     x.TargetKey == targetId,
                cancellationToken);
    }

    public async Task AddRangeAsync(
        List<RecommendationNotificationLog> entities,
        CancellationToken cancellationToken)
    {
        await _context.RecommendationNotificationLogs.AddRangeAsync(entities, cancellationToken);
    }

}

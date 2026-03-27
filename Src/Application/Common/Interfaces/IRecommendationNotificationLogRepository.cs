using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Interfaces;

public interface IRecommendationNotificationLogRepository:IRepository<RecommendationNotificationLog,int>
{
    Task<bool> ExistsAsync(string userId,RecommendationTargetType targetType,string targetId,CancellationToken cancellationToken);

    Task AddRangeAsync(List<RecommendationNotificationLog> entities,CancellationToken cancellationToken);
}

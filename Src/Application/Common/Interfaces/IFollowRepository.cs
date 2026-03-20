using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IFollowRepository:IRepository<Follow,int>
{
    Task<bool> ExistsAsync(string followerId, string followingId, CancellationToken ct);
    Task<Follow?> GetAsync(string followerId, string followingId, CancellationToken ct);
    Task<int> GetFollowersCountAsync(string userId, CancellationToken ct);
    Task<int> GetFollowingsCountAsync(string userId, CancellationToken ct);
    Task<List<string>> GetFollowerIdsAsync(string userId, int page, int pageSize, CancellationToken cancellationToken);
    Task<List<string>> GetFollowingIdsAsync(string userId, int page, int pageSize, CancellationToken cancellationToken);
    Task<int> GetMutualFollowingsTotalCountAsync(string currentUserId,string targetUserId,
    CancellationToken cancellationToken);
    Task<List<string>> GetMutualFollowingIdsAsync(string currentUserId,string targetUserId,int page,int pageSize,
    CancellationToken cancellationToken);
}

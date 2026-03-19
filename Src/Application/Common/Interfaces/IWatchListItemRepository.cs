using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IWatchListItemRepository:IRepository<WatchListItem,int>
{
    Task<WatchListItem?> GetByMovieAndUserAsync(int movieId, string userId, CancellationToken cancellationToken);
    Task<List<WatchListItem>> GetByUserIdAsync(string userId, CancellationToken cancellationToken);
    Task<List<WatchListItem>> GetPagedByUserIdAsync(string userId,int page,int pageSize,
    CancellationToken cancellationToken);
    Task<int> GetCountByUserIdAsync(string userId, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(int movieId, string userId, CancellationToken cancellationToken);
}

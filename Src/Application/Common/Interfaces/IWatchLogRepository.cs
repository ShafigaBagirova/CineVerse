using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Application.Common.Interfaces;

public interface IWatchLogRepository:IRepository<WatchLog,int>
{
    Task<WatchLog?> GetByMovieAndUserAsync(int movieId, string userId, CancellationToken cancellationToken);
    Task<List<WatchLog>> GetByUserIdAsync(string userId, CancellationToken cancellationToken);
    Task<List<WatchLog>> GetPagedByUserIdAsync( string userId,int page,int pageSize,
    CancellationToken cancellationToken);
    Task<int> GetCountByUserIdAsync(string userId, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(int movieId, string userId, CancellationToken cancellationToken);
}

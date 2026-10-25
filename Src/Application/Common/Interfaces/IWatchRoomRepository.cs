using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IWatchRoomRepository : IRepository<WatchRoom, int>
{
    Task<WatchRoom?> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task<WatchRoom?> GetByIdWithMembersAsync(int id, CancellationToken cancellationToken);
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken);
    Task<(List<WatchRoom> Items, int TotalCount)> GetPagedByUserAsync(
        string userId,
        int pageNumber,
        int pageSize,
        string? search,
        string? sortBy,
        bool desc,
        CancellationToken cancellationToken);
}

using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class WatchRoomRepository : GenericRepository<WatchRoom, int>, IWatchRoomRepository
{
    private readonly CineVerseDbContext _context;

    public WatchRoomRepository(CineVerseDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<WatchRoom?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        return await _context.WatchRooms
            .Include(r => r.Members)
            .FirstOrDefaultAsync(r => r.Code == code, cancellationToken);
    }

    public async Task<WatchRoom?> GetByIdWithMembersAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.WatchRooms
            .Include(r => r.Members)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken)
    {
        return await _context.WatchRooms.AnyAsync(r => r.Code == code, cancellationToken);
    }

    public async Task<(List<WatchRoom> Items, int TotalCount)> GetPagedByUserAsync(
        string userId,
        int pageNumber,
        int pageSize,
        string? search,
        string? sortBy,
        bool desc,
        CancellationToken cancellationToken)
    {
        IQueryable<WatchRoom> query = _context.WatchRooms
            .AsNoTracking()
            .Include(r => r.Members)
            .Where(r => r.IsActive && r.Members.Any(m => m.UserId == userId));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();
            query = query.Where(r => r.Title != null && r.Title.ToLower().Contains(normalizedSearch));
        }

        query = sortBy?.Trim().ToLower() switch
        {
            "title" => desc
                ? query.OrderByDescending(r => r.Title)
                : query.OrderBy(r => r.Title),

            _ => desc
                ? query.OrderByDescending(r => r.CreatedAt)
                : query.OrderBy(r => r.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}

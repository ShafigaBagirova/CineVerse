using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class WatchLogRepository:GenericRepository<WatchLog,int>, IWatchLogRepository
{
    private readonly CineVerseDbContext _context;

    public WatchLogRepository(CineVerseDbContext context) : base(context)
    {
        _context = context;
    }
    public async Task<WatchLog?> GetByMovieAndUserAsync(
        int movieId,
        string userId,
        CancellationToken cancellationToken)
    {
        return await _context.WatchLogs
            .Include(x => x.Movie)
            .FirstOrDefaultAsync(
                x => x.MovieId == movieId && x.UserId == userId,
                cancellationToken);
    }

    public async Task<List<WatchLog>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        return await _context.WatchLogs
            .Include(x => x.Movie)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<WatchLog>> GetPagedByUserIdAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        return await _context.WatchLogs
            .Include(x => x.Movie)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountByUserIdAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        return await _context.WatchLogs
            .Where(x => x.UserId == userId)
            .CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        int movieId,
        string userId,
        CancellationToken cancellationToken)
    {
        return await _context.WatchLogs
            .AnyAsync(x => x.MovieId == movieId && x.UserId == userId, cancellationToken);
    }
}

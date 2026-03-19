using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class WatchListItemRepository:GenericRepository<WatchListItem,int>, IWatchListItemRepository
{
    private readonly CineVerseDbContext _context;
    public WatchListItemRepository(CineVerseDbContext context):base(context)
    {
        _context = context;
    }
    public async Task<WatchListItem?> GetByMovieAndUserAsync(
       int movieId,
       string userId,
       CancellationToken cancellationToken)
    {
        return await _context.WatchlistItems
            .Include(x => x.Movie)
            .FirstOrDefaultAsync(
                x => x.MovieId == movieId && x.UserId == userId,
                cancellationToken);
    }

    public async Task<List<WatchListItem>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        return await _context.WatchlistItems
            .Include(x => x.Movie)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<WatchListItem>> GetPagedByUserIdAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        return await _context.WatchlistItems
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
        return await _context.WatchlistItems
            .Where(x => x.UserId == userId)
            .CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        int movieId,
        string userId,
        CancellationToken cancellationToken)
    {
        return await _context.WatchlistItems
            .AnyAsync(
                x => x.MovieId == movieId && x.UserId == userId,
                cancellationToken);
    }

}

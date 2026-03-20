using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class FollowRepository : GenericRepository<Follow, int>, IFollowRepository
{
    private readonly CineVerseDbContext _context;

    public FollowRepository(CineVerseDbContext context) : base(context)
    {
        _context = context;
    }
    public async Task<bool> ExistsAsync(string followerId, string followingId, CancellationToken ct)
    {
        return await _context.Follows
            .AnyAsync(x => x.FollowerId == followerId && x.FollowingId == followingId, ct);
    }
    public async Task<Follow?> GetAsync(string followerId, string followingId, CancellationToken ct)
    {
        return await _context.Follows
            .FirstOrDefaultAsync(x => x.FollowerId == followerId && x.FollowingId == followingId, ct);
    }
    public async Task<int> GetFollowersCountAsync(string userId, CancellationToken ct)
    {
        return await _context.Follows
            .CountAsync(x => x.FollowingId == userId, ct);
    }

    public async Task<int> GetFollowingsCountAsync(string userId, CancellationToken ct)
    {
        return await _context.Follows
            .CountAsync(x => x.FollowerId == userId, ct);
    }
    public async Task<List<string>> GetFollowerIdsAsync(string userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        return await _context.Follows
            .Where(x => x.FollowingId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => x.FollowerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<string>> GetFollowingIdsAsync(string userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        return await _context.Follows
            .Where(x => x.FollowerId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => x.FollowingId)
            .ToListAsync(cancellationToken);
    }
    public async Task<int> GetMutualFollowingsTotalCountAsync(
    string currentUserId,
    string targetUserId,
    CancellationToken cancellationToken)
    {
        var currentUserFollowingIds = _context.Follows
            .Where(x => x.FollowerId == currentUserId)
            .Select(x => x.FollowingId);

        var targetUserFollowingIds = _context.Follows
            .Where(x => x.FollowerId == targetUserId)
            .Select(x => x.FollowingId);

        return await currentUserFollowingIds
            .Intersect(targetUserFollowingIds)
            .CountAsync(cancellationToken);
    }

    public async Task<List<string>> GetMutualFollowingIdsAsync(
        string currentUserId,
        string targetUserId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var currentUserFollowingIds = _context.Follows
            .Where(x => x.FollowerId == currentUserId)
            .Select(x => x.FollowingId);

        var targetUserFollowingIds = _context.Follows
            .Where(x => x.FollowerId == targetUserId)
            .Select(x => x.FollowingId);

        return await currentUserFollowingIds
            .Intersect(targetUserFollowingIds)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
}

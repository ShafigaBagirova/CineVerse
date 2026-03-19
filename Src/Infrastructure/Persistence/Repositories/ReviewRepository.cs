using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ReviewRepository:GenericRepository<Review,int>,IReviewRepository
{
    private readonly CineVerseDbContext _context;

    public ReviewRepository(CineVerseDbContext context):base(context)
    {
        _context = context;
    }
    public async Task<Review?> GetByMovieAndUserAsync(
       int movieId,
       string userId,
       CancellationToken cancellationToken)
    {
        return await _context.Reviews
            .FirstOrDefaultAsync(
                x => x.MovieId == movieId &&
                     x.UserId == userId,
                cancellationToken);
    }

    public async Task<List<Review>> GetByMovieIdAsync(
        int movieId,
        CancellationToken cancellationToken)
    {
        return await _context.Reviews
            .Where(x => x.MovieId == movieId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
    public async Task<int> GetCountByMovieIdAsync(
    int movieId,
    CancellationToken cancellationToken)
    {
        return await _context.Reviews
            .Where(x => x.MovieId == movieId && !x.IsDeleted)
            .CountAsync(cancellationToken);
    }

    public async Task<List<Review>> GetPagedByMovieIdAsync(
        int movieId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        return await _context.Reviews
            .Where(x => x.MovieId == movieId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
    public async Task<int> GetCountByUserIdAsync(
       string userId,
       CancellationToken cancellationToken)
    {
        return await _context.Reviews
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .CountAsync(cancellationToken);
    }

    public async Task<List<Review>> GetPagedByUserIdAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        return await _context.Reviews
            .Where(x => x.UserId == userId && !x.IsDeleted)
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
}

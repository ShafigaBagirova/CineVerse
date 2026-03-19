using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Infrastructure.Persistence.Repositories;

public class MovieRatingRepository :  GenericRepository<MovieRating, int>, IMovieRatingRepository
{
    private readonly CineVerseDbContext _context;

    public MovieRatingRepository(CineVerseDbContext context): base(context)
    {
        _context = context; 
    }
    public async Task<decimal?> GetAverageRatingAsync(int movieId, CancellationToken cancellationToken)
    {
        var ratings = _context.MovieRatings.Where(x => x.MovieId == movieId);

        if (!await ratings.AnyAsync(cancellationToken))
            return null;

        return await ratings.AverageAsync(x => (decimal)x.Rating, cancellationToken);
    }
    public async Task<MovieRating?> GetByMovieAndUserAsync(
        int movieId,
        string userId,
        CancellationToken cancellationToken)
    {
        return await _context.MovieRatings
            .FirstOrDefaultAsync(x => x.MovieId == movieId && x.UserId == userId, cancellationToken);
    }
    public async Task<int> GetRatingsCountAsync(int movieId, CancellationToken cancellationToken)
    {
        return await _context.MovieRatings
            .CountAsync(x => x.MovieId == movieId, cancellationToken);
    }
}

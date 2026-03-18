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
    public async Task<decimal> GetAverageRatingAsync(int movieId, CancellationToken cancellationToken)
    {
        var average = await _context.MovieRatings
      .Where(x => x.MovieId == movieId)
      .Select(x => (decimal?)x.Rating)
      .AverageAsync(cancellationToken);

        return average ?? 0;
    }

    public async Task<MovieRating?> GetByMovieAndUserAsync(
        int movieId,
        string userId,
        CancellationToken cancellationToken)
    {
        return await _context.MovieRatings
            .FirstOrDefaultAsync(x => x.MovieId == movieId && x.UserId == userId, cancellationToken);
    }
}

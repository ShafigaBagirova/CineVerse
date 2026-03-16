using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;


public class MovieVideoRepository : GenericRepository<MovieVideo, int>, IMovieVideoRepository
{
    private readonly CineVerseDbContext _context;

    public MovieVideoRepository(CineVerseDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<MovieVideo?> GetTrailerByMovieIdAsync(int movieId, CancellationToken cancellationToken = default)
    {
        return await _context.Set<MovieVideo>()
            .FirstOrDefaultAsync(x => x.MovieId == movieId && x.Type == "Trailer", cancellationToken);
    }
}
using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class MoviePosterRepository :GenericRepository<MoviePoster,int>, IMoviePosterRepository
{
    private readonly CineVerseDbContext _context;

    public MoviePosterRepository(CineVerseDbContext context):base(context)
    {
        _context = context;
    }

    public async Task<List<MoviePoster>> GetByMovieIdAsync(int movieId, CancellationToken ct)
    {
        return await _context.MoviePosters
            .Where(x => x.MovieId == movieId)
            .OrderBy(x => x.Order)
            .ToListAsync(ct);
    }


}


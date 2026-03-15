using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class MovieRepository : IMovieRepository
{
    private readonly CineVerseDbContext _context;

    public MovieRepository(CineVerseDbContext context)
    {
        _context = context;
    }

    public async Task<Movie?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Movies
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<Movie?> GetByTmdbIdAsync(long tmdbId, CancellationToken cancellationToken = default)
    {
        return await _context.Movies
            .FirstOrDefaultAsync(x => x.TmdbId == tmdbId, cancellationToken);
    }

    public async Task<List<Movie>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Movies
        .AsNoTracking()
        .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        await _context.Movies.AddAsync(movie, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        _context.Movies.Update(movie);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

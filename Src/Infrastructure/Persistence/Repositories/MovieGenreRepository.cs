using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class MovieGenreRepository : IMovieGenreRepository
{
    private readonly CineVerseDbContext _context;

    public MovieGenreRepository(CineVerseDbContext context)
    {
        _context = context;
    }

    public async Task<List<MovieGenre>> GetByMovieIdAsync(int movieId, CancellationToken cancellationToken)
    {
        return await _context.MovieGenres
            .Where(x => x.MovieId == movieId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int movieId, int genreId, CancellationToken cancellationToken)
    {
        return await _context.MovieGenres
            .AnyAsync(x => x.MovieId == movieId && x.GenreId == genreId, cancellationToken);
    }

    public async Task AddAsync(MovieGenre movieGenre, CancellationToken cancellationToken)
    {
        await _context.MovieGenres.AddAsync(movieGenre, cancellationToken);
    }

    public Task RemoveRangeAsync(List<MovieGenre> movieGenres, CancellationToken cancellationToken)
    {
        _context.MovieGenres.RemoveRange(movieGenres);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
    public async Task<MovieGenre?> GetPrimaryByMovieIdAsync(int movieId, CancellationToken cancellationToken)
    {
        return await _context.MovieGenres
            .FirstOrDefaultAsync(
                x => x.MovieId == movieId && x.IsPrimary,
                cancellationToken);
    }
    public async Task<MovieGenre?> GetByIdsAsync(int movieId, int genreId, CancellationToken cancellationToken)
    {
        return await _context.MovieGenres
            .FirstOrDefaultAsync(
                x => x.MovieId == movieId && x.GenreId == genreId,
                cancellationToken);
    }
    public Task UpdateAsync(MovieGenre movieGenre, CancellationToken cancellationToken)
    {
        _context.MovieGenres.Update(movieGenre);
        return Task.CompletedTask;
    }
    public Task DeleteAsync(MovieGenre movieGenre, CancellationToken cancellationToken)
    {
        _context.MovieGenres.Remove(movieGenre);
        return Task.CompletedTask;
    }
}

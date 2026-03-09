using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class MoviePosterRepository : IMoviePosterRepository
{
    private readonly CineVerseDbContext _context;

    public MoviePosterRepository(CineVerseDbContext context)
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

    public async Task AddAsync(MoviePoster media, CancellationToken ct)
    {
        await _context.MoviePosters.AddAsync(media, ct);
    }

    public Task DeleteAsync(MoviePoster media, CancellationToken ct)
    {
        _context.MoviePosters.Remove(media);
        return Task.CompletedTask;
    }
}


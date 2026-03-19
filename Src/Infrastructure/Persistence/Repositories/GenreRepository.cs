using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class GenreRepository : GenericRepository<Genre,int>,IGenreRepository
{
    private readonly CineVerseDbContext _context;

    public GenreRepository(CineVerseDbContext context): base(context)
    {
        _context = context;
    }

    public async Task<Genre?> GetByTmdbGenreIdAsync(int tmdbGenreId, CancellationToken cancellationToken)
    {
        return await _context.Genres
            .FirstOrDefaultAsync(x => x.TmdbGenreId == tmdbGenreId, cancellationToken);
    }
    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Genres
            .AnyAsync(x => x.Id == id, cancellationToken);
    }
    public async Task<Genre?> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        return await _context.Genres
            .FirstOrDefaultAsync(x => x.Name == name, cancellationToken);
    }

}
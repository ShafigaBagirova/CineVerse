using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class MovieRepository :GenericRepository<Movie,int>, IMovieRepository
{
    private readonly CineVerseDbContext _context;

    public MovieRepository(CineVerseDbContext context): base(context)
    {
        _context = context;
    }


    public async Task<Movie?> GetByTmdbIdAsync(long tmdbId, CancellationToken cancellationToken = default)
    {
        return await _context.Movies
            .FirstOrDefaultAsync(x => x.TmdbId == tmdbId, cancellationToken);
    }
    public async Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        return await _context.Movies
            .AnyAsync(x => x.Slug == slug, cancellationToken);
    }

    public async Task<bool> ExistsByTmdbIdAsync(long tmdbId, CancellationToken cancellationToken)
    {
        return await _context.Movies
            .AnyAsync(x => x.TmdbId == tmdbId, cancellationToken);
    }

    public async Task<bool> ExistsByTitleAndReleaseDateAsync(string title,DateOnly? releaseDate,CancellationToken cancellationToken)
    {
        return await _context.Movies
            .AnyAsync( x => x.Title == title && x.ReleaseDate == releaseDate,cancellationToken);
    }
    public async Task<Movie?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Movies
            .FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);
    }
    public async Task<List<Movie>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        MovieSortBy sortBy,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Movies
            .AsQueryable();

        switch (sortBy)
        {
            case MovieSortBy.UserRating:
                query = query.OrderByDescending(x => x.UserAverageRating);
                break;

            case MovieSortBy.Title:
                query = query.OrderBy(x => x.Title);
                break;

            default:
                query = query.OrderByDescending(x => x.ReleaseDate);
                break;
        }

        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Movies.CountAsync(cancellationToken);
    }
    public async Task<Movie?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Movies
    .Include(m => m.MovieGenres)
        .ThenInclude(mg => mg.Genre)
    .Include(m => m.Reviews.Where(r => !r.IsDeleted))
    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
    public async Task<int> CountByStatusAsync(MovieStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Movies
            .CountAsync(x => x.Status == status, cancellationToken);
    }

    public async Task<int> CountSearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        searchTerm = searchTerm.ToLower();

        return await _context.Movies
            .CountAsync(x =>
                x.Title.ToLower().Contains(searchTerm) ||
                x.Description.ToLower().Contains(searchTerm) ||
                (x.Tagline != null && x.Tagline.ToLower().Contains(searchTerm)),
                cancellationToken);
    }
    public async Task<List<Movie>> GetByStatusPagedAsync( MovieStatus status,int pageNumber, int pageSize,
    CancellationToken cancellationToken = default)
    {
        return await _context.Movies
            .Where(x => x.Status == status)
            .OrderByDescending(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
    public async Task<List<Movie>> SearchPagedAsync( string searchTerm,int pageNumber, int pageSize,
    CancellationToken cancellationToken = default)
    {
        searchTerm = searchTerm.ToLower();

        return await _context.Movies
            .Where(x =>
           x.Title.ToLower().Contains(searchTerm) ||
           x.Description.ToLower().Contains(searchTerm) ||
           (x.Tagline != null && x.Tagline.ToLower().Contains(searchTerm)) ||
          (x.Director != null && x.Director.ToLower().Contains(searchTerm)))
            .OrderByDescending(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByLanguageAsync(string language, CancellationToken cancellationToken = default)
    {
        language = language.Trim().ToLower();

        return await _context.Movies
            .CountAsync(x => x.Language != null && x.Language.ToLower() == language, cancellationToken);
    }

    public async Task<List<Movie>> GetByLanguagePagedAsync(string language, int pageNumber,int pageSize,
        CancellationToken cancellationToken = default)
    {
        language = language.Trim().ToLower();

        return await _context.Movies
            .Where(x => x.Language != null && x.Language.ToLower() == language)
            .OrderByDescending(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
    public async Task<int> CountByYearAsync(int year, CancellationToken cancellationToken = default)
    {
        return await _context.Movies
            .CountAsync(x => x.ReleaseDate.HasValue && x.ReleaseDate.Value.Year == year, cancellationToken);
    }

    public async Task<List<Movie>> GetByYearPagedAsync(
        int year,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.Movies
            .Where(x => x.ReleaseDate.HasValue && x.ReleaseDate.Value.Year == year)
            .OrderByDescending(x => x.ReleaseDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
    public async Task<int> CountByUserRatingRangeAsync(
    decimal minRating,
    decimal maxRating,
    CancellationToken cancellationToken = default)
    {
        return await _context.Movies
            .CountAsync(x =>
                x.UserAverageRating.HasValue &&
                x.UserAverageRating.Value >= minRating &&
                x.UserAverageRating.Value <= maxRating,
                cancellationToken);
    }

    public async Task<List<Movie>> GetByUserRatingRangePagedAsync(
        decimal minRating,
        decimal maxRating,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.Movies
            .Where(x =>
                x.UserAverageRating.HasValue &&
                x.UserAverageRating.Value >= minRating &&
                x.UserAverageRating.Value <= maxRating)
            .OrderByDescending(x => x.UserAverageRating)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
    public async Task<int> CountByTmdbRatingRangeAsync(
    decimal minRating,
    decimal maxRating,
    CancellationToken cancellationToken = default)
    {
        return await _context.Movies
            .CountAsync(x =>
                x.TmdbRating.HasValue &&
                x.TmdbRating.Value >= minRating &&
                x.TmdbRating.Value <= maxRating,
                cancellationToken);
    }

    public async Task<List<Movie>> GetByTmdbRatingRangePagedAsync(
        decimal minRating,
        decimal maxRating,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.Movies
            .Where(x =>
                x.TmdbRating.HasValue &&
                x.TmdbRating.Value >= minRating &&
                x.TmdbRating.Value <= maxRating)
            .OrderByDescending(x => x.TmdbRating)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
    public async Task<List<Movie>> GetMoviesByGenreAsync(
    int genreId,
    int page,
    int pageSize,
    CancellationToken cancellationToken)
    {
        return await _context.Movies
            .AsNoTracking()
            .Where(m => m.MovieGenres.Any(mg => mg.GenreId == genreId))
            .OrderByDescending(m => m.UserAverageRating)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetMoviesCountByGenreAsync(
        int genreId,
        CancellationToken cancellationToken)
    {
        return await _context.Movies
            .AsNoTracking()
            .CountAsync(m => m.MovieGenres.Any(mg => mg.GenreId == genreId), cancellationToken);
    }
    
}

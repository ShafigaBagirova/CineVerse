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
    public async Task<(List<Movie> Items, int TotalCount)> GetPagedAsync(
       int pageNumber,
       int pageSize,
       string? search,
       string? actorName,
       string? directorName,
       int? genreId,
       string? language,
       MovieStatus? Status,
       int? year,
       decimal? minTmdbRating,
       decimal? maxTmdbRating,
       decimal? minUserRating,
       decimal? maxUserRating,
       string? sortBy,
       bool desc,
       CancellationToken cancellationToken)
    {
        IQueryable<Movie> query = _context.Movies
            .AsNoTracking()
            .Include(x => x.MovieGenres);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();

            query = query.Where(x =>
                x.Title.ToLower().Contains(normalizedSearch) ||
                (x.Actors != null && x.Actors.ToLower().Contains(normalizedSearch)) ||
                (x.Director != null && x.Director.ToLower().Contains(normalizedSearch)));
        }

        if (!string.IsNullOrWhiteSpace(actorName))
        {
            var actor = actorName.Trim().ToLower();
            query = query.Where(x =>
                x.Actors != null &&
                x.Actors.ToLower().Contains(actor));
        }

        if (!string.IsNullOrWhiteSpace(directorName))
        {
            var director = directorName.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Director != null &&
                x.Director.ToLower().Contains(director));
        }

        if (genreId.HasValue)
        {
            query = query.Where(x => x.MovieGenres.Any(mg => mg.GenreId == genreId.Value));
        }

        if (!string.IsNullOrWhiteSpace(language))
        {
            var normalizedLanguage = language.Trim().ToLower();
            query = query.Where(x => x.Language != null && x.Language.ToLower() == normalizedLanguage);
        }

        if (Status.HasValue)
        {
            query = query.Where(x => x.Status == Status.Value);
        }
        if (year.HasValue)
        {
            query = query.Where(x =>
                x.ReleaseDate.HasValue && x.ReleaseDate.Value.Year == year.Value);
        }

        if (minTmdbRating.HasValue)
        {
            query = query.Where(x => x.TmdbRating.HasValue && x.TmdbRating.Value >= minTmdbRating.Value);
        }

        if (maxTmdbRating.HasValue)
        {
            query = query.Where(x => x.TmdbRating.HasValue && x.TmdbRating.Value <= maxTmdbRating.Value);
        }

        if (minUserRating.HasValue)
        {
            query = query.Where(x => x.UserAverageRating.HasValue && x.UserAverageRating.Value >= minUserRating.Value);
        }

        if (maxUserRating.HasValue)
        {
            query = query.Where(x => x.UserAverageRating.HasValue && x.UserAverageRating.Value <= maxUserRating.Value);
        }

    
        query = sortBy?.Trim().ToLowerInvariant() switch
        {
            "title" => desc
                ? query.OrderByDescending(x => x.Title)
                : query.OrderBy(x => x.Title),

            "year" => desc
                ? query.OrderByDescending(x => x.ReleaseDate)
                : query.OrderBy(x => x.ReleaseDate),

            "tmdb_rating" or "tmdbrating" => desc
                ? query.OrderByDescending(x => x.TmdbRating)
                : query.OrderBy(x => x.TmdbRating),

            "user_rating" or "userrating" => desc
                ? query.OrderByDescending(x => x.UserAverageRating)
                : query.OrderBy(x => x.UserAverageRating),

            "createdat" => desc
                ? query.OrderByDescending(x => x.CreatedAt)
                : query.OrderBy(x => x.CreatedAt),

            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Movies.CountAsync(cancellationToken);
    }
    public async Task<Movie?> GetMovieWithDetailsByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Movies
    .Include(m => m.MovieGenres)
        .ThenInclude(mg => mg.Genre)
    .Include(m => m.Videos)
    .Include(m => m.Reviews.Where(r => !r.IsDeleted))
    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Movies
            .AnyAsync(x => x.Id == id, cancellationToken);
    }



}

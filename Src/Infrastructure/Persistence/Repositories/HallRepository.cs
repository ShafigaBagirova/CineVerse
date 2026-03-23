using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class HallRepository:GenericRepository<Hall,int>, IHallRepository
{
    private readonly CineVerseDbContext _context;
    public HallRepository(CineVerseDbContext context) : base(context)
    {
        _context = context;
    }
    public async Task<List<Hall>> GetAllActiveAsync(CancellationToken cancellationToken)
    {
        return await _context.Halls
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Hall>> GetByCinemaIdAsync(int cinemaId, CancellationToken cancellationToken)
    {
        return await _context.Halls
            .AsNoTracking()
            .Where(x => x.CinemaId == cinemaId && x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameInCinemaAsync(
        int cinemaId,
        string name,
        CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim().ToLower();

        return await _context.Halls
            .AnyAsync(x =>
                x.CinemaId == cinemaId &&
                x.Name.ToLower() == normalizedName,
                cancellationToken);
    }

    public async Task<bool> ExistsByNameInCinemaAsync(
        int cinemaId,
        string name,
        int excludeHallId,
        CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim().ToLower();

        return await _context.Halls
            .AnyAsync(x =>
                x.CinemaId == cinemaId &&
                x.Id != excludeHallId &&
                x.Name.ToLower() == normalizedName,
                cancellationToken);
    }
    public async Task<(List<Hall> Items, int TotalCount)> GetPagedAsync(
     int pageNumber,
     int pageSize,
     int? cinemaId,
     string? search,
     string? sortBy,
     bool desc,
     CancellationToken cancellationToken)
    {
        IQueryable<Hall> query = _context.Halls
            .AsNoTracking()
            .Where(x => x.IsActive);

        if (cinemaId.HasValue)
        {
            query = query.Where(x => x.CinemaId == cinemaId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(normalizedSearch));
        }

        query = sortBy?.Trim().ToLower() switch
        {
            "name" => desc
                ? query.OrderByDescending(x => x.Name)
                : query.OrderBy(x => x.Name),

            "capacity" => desc
                ? query.OrderByDescending(x => x.Capacity)
                : query.OrderBy(x => x.Capacity),

            _ => query.OrderBy(x => x.Name)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Halls
            .AnyAsync(h => h.Id == id, cancellationToken);
    }
}

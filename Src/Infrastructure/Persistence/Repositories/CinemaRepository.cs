using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class CinemaRepository:GenericRepository<Cinema,int>, ICinemaRepository
{
    private readonly CineVerseDbContext _context;

    public CinemaRepository(CineVerseDbContext context) : base(context)
    {
        _context = context;
    }
    public async Task<(List<Cinema> Items, int TotalCount)> GetPagedActiveAsync(
       int pageNumber,
       int pageSize,
       CancellationToken cancellationToken)
    {
        var query = _context.Cinemas
            .AsNoTracking()
            .Where(x => x.IsActive);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<List<Cinema>> GetByLocationAsync(
        string? country,
        string? city,
        CancellationToken cancellationToken)
    {
        var query = _context.Cinemas
            .AsNoTracking()
            .Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(country))
        {
            var trimmedCountry = country.Trim();
            query = query.Where(x => x.Country == trimmedCountry);
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            var trimmedCity = city.Trim();
            query = query.Where(x => x.City == trimmedCity);
        }

        return await query
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
}

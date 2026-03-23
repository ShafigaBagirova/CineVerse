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
          string? country,
          string? city,
          string? search,
          string? sortBy,
          bool desc,
          CancellationToken cancellationToken)
    {
        IQueryable<Cinema> query = _context.Cinemas
            .AsNoTracking()
            .Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(country))
        {
            var normalizedCountry = country.Trim().ToLower();
            query = query.Where(x => x.Country.ToLower() == normalizedCountry);
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            var normalizedCity = city.Trim().ToLower();
            query = query.Where(x => x.City.ToLower() == normalizedCity);
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

            "city" => desc
                ? query.OrderByDescending(x => x.City)
                : query.OrderBy(x => x.City),

            "country" => desc
                ? query.OrderByDescending(x => x.Country)
                : query.OrderBy(x => x.Country),

            _ => query.OrderBy(x => x.Name)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}

using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class FoodCategoryRepository
    : GenericRepository<FoodCategory, int>, IFoodCategoryRepository
{
    private readonly CineVerseDbContext _context;

    public FoodCategoryRepository(CineVerseDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<FoodCategory>> GetAllActiveWithItemsAsync(CancellationToken cancellationToken)
    {
        return await _context.FoodCategories
            .AsNoTracking()
            .Where(x => x.IsActive)
            .Include(x => x.FoodItems.Where(fi => fi.IsActive && fi.IsAvailable))
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }
    public Task<IQueryable<FoodCategory>> GetQueryableAsync()
    {
        IQueryable<FoodCategory> query = _context.FoodCategories.AsNoTracking();
        return Task.FromResult(query);
    }

    public async Task<List<FoodCategory>> ToListAsync(
        IQueryable<FoodCategory> query,
        CancellationToken cancellationToken = default)
    {
        return await query.ToListAsync(cancellationToken);
    }
    public async Task<List<FoodCategory>> GetCategoriesWithItemsAsync(
       int? cinemaId,
       bool? isActive,
       string? search,
       CancellationToken cancellationToken = default)
    {
        var query = _context.FoodCategories
            .AsNoTracking()
            .Include(x => x.FoodItems.Where(fi => fi.IsAvailable))
            .AsQueryable();

        if (cinemaId.HasValue)
        {
            query = query.Where(x => x.CinemaId == cinemaId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();

            query = query.Where(x =>
                x.Name.ToLower().Contains(normalizedSearch) ||
                (x.Description != null && x.Description.ToLower().Contains(normalizedSearch)));
        }

        return await query
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);
    }
}
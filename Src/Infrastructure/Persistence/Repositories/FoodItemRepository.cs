using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class FoodItemRepository: GenericRepository<FoodItem, int>, IFoodItemRepository
{
    private readonly CineVerseDbContext _context;

    public FoodItemRepository(CineVerseDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<FoodItem>> GetAllActiveAsync(CancellationToken cancellationToken)
    {
        return await _context.FoodItems
            .AsNoTracking()
            .Where(x => x.IsActive && x.IsAvailable)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<FoodItem>> GetByIdsAsync(
        List<int> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
            return [];

        return await _context.FoodItems
            .AsNoTracking()
            .Where(x => ids.Contains(x.Id) && x.IsActive && x.IsAvailable)
            .ToListAsync(cancellationToken);
    }
    public Task<IQueryable<FoodItem>> GetQueryableAsync()
    {
        IQueryable<FoodItem> query = _context.FoodItems
            .AsNoTracking()
            .Include(x => x.FoodCategory);

        return Task.FromResult(query);
    }

    public async Task<List<FoodItem>> ToListAsync(
        IQueryable<FoodItem> query,
        CancellationToken cancellationToken = default)
    {
        return await query.ToListAsync(cancellationToken);
    }

}
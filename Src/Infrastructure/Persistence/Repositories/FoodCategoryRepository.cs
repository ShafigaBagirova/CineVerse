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
}
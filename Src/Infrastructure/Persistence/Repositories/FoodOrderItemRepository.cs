using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Context;

namespace Infrastructure.Persistence.Repositories;

public sealed class FoodOrderItemRepository: GenericRepository<FoodOrderItem, int>, IFoodOrderItemRepository
{
    private readonly CineVerseDbContext _context;

    public FoodOrderItemRepository(CineVerseDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task AddRangeAsync(List<FoodOrderItem> items, CancellationToken cancellationToken)
    {
        await _context.FoodOrderItems.AddRangeAsync(items, cancellationToken);
    }
}
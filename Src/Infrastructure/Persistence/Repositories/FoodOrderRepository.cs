using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public sealed class FoodOrderRepository
    : GenericRepository<FoodOrder, int>, IFoodOrderRepository
{
    private readonly CineVerseDbContext _context;

    public FoodOrderRepository(CineVerseDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<FoodOrder?> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.FoodOrders
            .Include(x => x.FoodOrderItems)
                .ThenInclude(x => x.FoodItem)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<FoodOrder?> GetPendingBySeatHoldIdAsync(
        int seatHoldId,
        CancellationToken cancellationToken)
    {
        return await _context.FoodOrders
            .Include(x => x.FoodOrderItems)
            .FirstOrDefaultAsync(
                x => x.SeatHoldId == seatHoldId &&
                     x.Status == FoodOrderStatus.Pending,
                cancellationToken);
    }

    public async Task<List<FoodOrder>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        return await _context.FoodOrders
            .AsNoTracking()
            .Include(x => x.FoodOrderItems)
                .ThenInclude(x => x.FoodItem)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<FoodOrder>> GetByScreeningIdAsync(
        int screeningId,
        CancellationToken cancellationToken)
    {
        return await _context.FoodOrders
            .AsNoTracking()
            .Include(x => x.FoodOrderItems)
                .ThenInclude(x => x.FoodItem)
            .Where(x => x.ScreeningId == screeningId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsPendingBySeatHoldIdAsync(
        int seatHoldId,
        CancellationToken cancellationToken)
    {
        return await _context.FoodOrders
            .AsNoTracking()
            .AnyAsync(
                x => x.SeatHoldId == seatHoldId &&
                     x.Status == FoodOrderStatus.Pending,
                cancellationToken);
    }

    public async Task<List<FoodOrder>> GetPendingByScreeningIdAsync(
        int screeningId,
        CancellationToken cancellationToken)
    {
        return await _context.FoodOrders
            .Include(x => x.FoodOrderItems)
                .ThenInclude(x => x.FoodItem)
            .Where(x =>
                x.ScreeningId == screeningId &&
                (x.Status == FoodOrderStatus.Pending ||
                 x.Status == FoodOrderStatus.Confirmed ||
                 x.Status == FoodOrderStatus.Preparing ||
                 x.Status == FoodOrderStatus.Ready))
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
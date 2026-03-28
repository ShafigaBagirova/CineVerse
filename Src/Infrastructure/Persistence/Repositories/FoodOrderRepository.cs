using Application.Common.Interfaces;
using Application.FoodOrders.Dtos;
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
    public async Task<List<FoodOrder>> ToListAsync(
       IQueryable<FoodOrder> query,
       CancellationToken cancellationToken = default)
    {
        return await query.ToListAsync(cancellationToken);
    }

    public Task<IQueryable<FoodOrder>> GetQueryableAsync()
    {
        IQueryable<FoodOrder> query = _context.FoodOrders
            .AsNoTracking()
            .Include(x => x.FoodOrderItems)
                .ThenInclude(x => x.FoodItem);

        return Task.FromResult(query);
    }

    public async Task<List<OrdersByDayResponse>> GetOrdersByDayAsync(
        int? cinemaId,
        int? screeningId,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var query = _context.FoodOrders
            .AsNoTracking()
            .Where(x => x.Status == FoodOrderStatus.Confirmed ||
                        x.Status == FoodOrderStatus.Delivered)
            .AsQueryable();

        if (cinemaId.HasValue)
        {
            query = query.Where(x => x.CinemaId == cinemaId.Value);
        }

        if (screeningId.HasValue)
        {
            query = query.Where(x => x.ScreeningId == screeningId.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= to.Value);
        }

        return await query
            .GroupBy(x => x.CreatedAt.Date)
            .Select(g => new OrdersByDayResponse
            {
                Date = g.Key,
                OrderCount = g.Count(),
                TotalRevenue = g.Sum(x => x.TotalAmount)
            })
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);
    }
    public async Task<FoodOrder?> GetActiveBySeatHoldIdAsync(
    int seatHoldId,
    CancellationToken cancellationToken = default)
    {
        return await _context.FoodOrders
            .AsNoTracking()
            .Include(x => x.FoodOrderItems)
            .FirstOrDefaultAsync(
                x => x.SeatHoldId == seatHoldId &&
                     x.Status != FoodOrderStatus.Cancelled &&
                     x.Status != FoodOrderStatus.Refunded,
                cancellationToken);
    }
}
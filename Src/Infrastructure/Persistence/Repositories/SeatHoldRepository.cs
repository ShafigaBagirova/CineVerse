using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class SeatHoldRepository:GenericRepository<SeatHold,int>, ISeatHoldRepository
{
    private readonly CineVerseDbContext _context;

    public SeatHoldRepository(CineVerseDbContext context):base(context)
    {
        _context = context;
    }
    public async Task<SeatHold?> GetActiveHoldAsync(int screeningId, int seatId, CancellationToken cancellationToken)
    {
        return await _context.SeatHolds
            .Where(x =>
                x.ScreeningId == screeningId &&
                x.SeatId == seatId &&
                x.Status == SeatHoldStatus.Active)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
    public async Task<(IReadOnlyList<SeatHold> Items, int TotalCount)> GetPagedAsync(int? userId,SeatHoldStatus? status,
    DateTime? expiresBeforeUtc,DateTime? expiresAfterUtc,int pageNumber,int pageSize,
    CancellationToken cancellationToken)
    {
        var query = _context.SeatHolds.AsNoTracking().AsQueryable();

        if (userId.HasValue)
            query = query.Where(x => x.UserId == userId.Value);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        if (expiresBeforeUtc.HasValue)
            query = query.Where(x => x.ExpiresAtUtc < expiresBeforeUtc.Value);

        if (expiresAfterUtc.HasValue)
            query = query.Where(x => x.ExpiresAtUtc > expiresAfterUtc.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
    public async Task<(IReadOnlyList<SeatHold> Items, int TotalCount)> GetByScreeningAsync(
    int screeningId,
    SeatHoldStatus? status,
    DateTime? expiresBeforeUtc,
    DateTime? expiresAfterUtc,
    int pageNumber,
    int pageSize,
    CancellationToken cancellationToken)
    {
        var query = _context.SeatHolds
            .AsNoTracking()
            .Where(x => x.ScreeningId == screeningId)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        if (expiresBeforeUtc.HasValue)
            query = query.Where(x => x.ExpiresAtUtc < expiresBeforeUtc.Value);

        if (expiresAfterUtc.HasValue)
            query = query.Where(x => x.ExpiresAtUtc > expiresAfterUtc.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

}

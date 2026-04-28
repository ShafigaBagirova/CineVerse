using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class TicketRepository:GenericRepository<Ticket,int>, ITicketRepository
{
    private readonly CineVerseDbContext _context;

    public TicketRepository(CineVerseDbContext context) : base(context)
    {
        _context = context;
    }
    public async Task<bool> ExistsByScreeningAndSeatAsync(int screeningId, int seatId, CancellationToken cancellationToken)
    {
        return await _context.Tickets
            .AnyAsync(x =>
                x.ScreeningId == screeningId &&
                x.SeatId == seatId,
                cancellationToken);
    }
    public async Task<(IReadOnlyList<Ticket> Items, int TotalCount)> GetMyTicketsAsync(string userId, TicketStatus? status,
    DateTime? purchasedAfterUtc, DateTime? purchasedBeforeUtc,int pageNumber,int pageSize,
    CancellationToken cancellationToken)
    {
        var query = _context.Tickets
            .AsNoTracking()
            .Include(x => x.Screening)
                .ThenInclude(x => x.Movie)
            .Include(x => x.Screening)
                .ThenInclude(x => x.Hall)
                    .ThenInclude(x => x.Cinema)
            .Include(x => x.Seat)
            .Where(x => x.UserId == userId)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        if (purchasedAfterUtc.HasValue)
            query = query.Where(x => x.PurchasedAtUtc >= purchasedAfterUtc.Value);

        if (purchasedBeforeUtc.HasValue)
            query = query.Where(x => x.PurchasedAtUtc <= purchasedBeforeUtc.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.PurchasedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
    public async Task<Ticket?> GetDetailedByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Tickets
            .AsNoTracking()
            .Include(x => x.Screening)
                .ThenInclude(x => x.Movie)
            .Include(x => x.Screening)
                .ThenInclude(x => x.Hall)
                    .ThenInclude(x => x.Cinema)
            .Include(x => x.Seat)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }
    public async Task<(IReadOnlyList<Ticket> Items, int TotalCount)> GetByScreeningAsync(
    int screeningId,
    TicketStatus? status,
    DateTime? purchasedAfterUtc,
    DateTime? purchasedBeforeUtc,
    int pageNumber,
    int pageSize,
    CancellationToken cancellationToken)
    {
        var query = _context.Tickets
            .AsNoTracking()
            .Include(x => x.Screening)
                .ThenInclude(x => x.Movie)
            .Include(x => x.Screening)
                .ThenInclude(x => x.Hall)
                    .ThenInclude(x => x.Cinema)
            .Include(x => x.Seat)
            .Where(x => x.ScreeningId == screeningId)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        if (purchasedAfterUtc.HasValue)
            query = query.Where(x => x.PurchasedAtUtc >= purchasedAfterUtc.Value);

        if (purchasedBeforeUtc.HasValue)
            query = query.Where(x => x.PurchasedAtUtc <= purchasedBeforeUtc.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.PurchasedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
    public async Task<List<int>> GetSoldSeatIdsByScreeningAsync(int screeningId, CancellationToken cancellationToken)
    {
        return await _context.Tickets
            .AsNoTracking()
            .Where(x => x.ScreeningId == screeningId && x.Status == TicketStatus.Paid)
            .Select(x => x.SeatId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
    public async Task<List<Ticket>> GetPaidTicketsByScreeningIdAsync(
      int screeningId,
      CancellationToken cancellationToken)
    {
        return await _context.Tickets
            .AsNoTracking()
            .Where(x =>
                x.ScreeningId == screeningId &&
                x.Status == TicketStatus.Paid)
            .ToListAsync(cancellationToken);
    }
    public async Task<int> CountSoldTicketsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Tickets.CountAsync(x => x.Status == TicketStatus.Paid, cancellationToken);
    }
}

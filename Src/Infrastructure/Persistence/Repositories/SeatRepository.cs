using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class SeatRepository:GenericRepository<Seat,int>,ISeatRepository
{
    private readonly CineVerseDbContext _context;
    public SeatRepository(CineVerseDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<bool> ExistsAsync(int hallId, string row, int number, int excludeSeatId, CancellationToken cancellationToken)
    {
        return await _context.Seats.AnyAsync(x => x.HallId == hallId &&x.Row == row &&x.Number == number &&x.Id != excludeSeatId,
        cancellationToken);
    }
    public async Task<bool> ExistsAsync(int hallId, string row, int number, CancellationToken cancellationToken)
    {
        return await _context.Seats.AnyAsync(x =>
            x.HallId == hallId &&
            x.Row == row &&
            x.Number == number,
            cancellationToken);
    }
    public IQueryable<Seat> GetAll()
    {
        return _context.Seats.AsNoTracking();
    }
    public async Task<List<Seat>> GetActiveByHallIdAsync(int hallId, CancellationToken cancellationToken)
    {
        return await _context.Seats
            .AsNoTracking()
            .Where(x => x.HallId == hallId && x.IsActive)
            .OrderBy(x => x.Row)
            .ThenBy(x => x.Number)
            .ToListAsync(cancellationToken);
    }
}

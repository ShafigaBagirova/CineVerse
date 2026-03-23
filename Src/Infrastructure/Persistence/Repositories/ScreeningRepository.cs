using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class ScreeningRepository:GenericRepository<Screening,int>, IScreeningRepository
{
    private readonly CineVerseDbContext _context;
    public ScreeningRepository(CineVerseDbContext context) : base(context)
    {
        _context = context;
    }
    public async Task<bool> HasTimeConflictAsync(int hallId,DateTime startTime, DateTime endTime,
       CancellationToken cancellationToken)
    {
        return await _context.Screenings
            .AnyAsync(x =>
                    x.HallId == hallId &&
                    x.IsActive &&
                    x.Status != ScreeningStatus.Cancelled &&
                    x.StartTime < endTime &&
                    x.EndTime > startTime,
                cancellationToken);
    }
    public async Task<bool> HasTimeConflictAsync(int hallId,DateTime startTime,DateTime endTime,
       int excludeScreeningId,
       CancellationToken cancellationToken)
    {
        return await _context.Screenings
            .AnyAsync(x =>
                    x.Id != excludeScreeningId &&
                    x.HallId == hallId &&
                    x.IsActive &&
                    x.Status != ScreeningStatus.Cancelled &&
                    x.StartTime < endTime &&
                    x.EndTime > startTime,
                cancellationToken);
    }
    public async Task<Screening?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Screenings
            .Include(x => x.Movie)
            .Include(x => x.Hall)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive, cancellationToken);
    }
    public async Task<(List<Screening> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize,int? movieId,
     int? hallId, ScreeningStatus? status,ScreeningFormat? format,bool? isActive,DateTime? dateFrom,DateTime? dateTo,
       CancellationToken cancellationToken)
    {
        var query = _context.Screenings
            .AsNoTracking()
            .Include(x => x.Movie)
            .Include(x => x.Hall)
            .AsQueryable();

        if (movieId.HasValue)
        {
            query = query.Where(x => x.MovieId == movieId.Value);
        }

        if (hallId.HasValue)
        {
            query = query.Where(x => x.HallId == hallId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (format.HasValue)
        {
            query = query.Where(x => x.Format == format.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(x => x.StartTime >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            query = query.Where(x => x.StartTime <= dateTo.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.StartTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}

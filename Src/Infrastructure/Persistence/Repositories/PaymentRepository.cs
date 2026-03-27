using Application.Common.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class PaymentRepository:GenericRepository<Payment,int>,IPaymentRepository
{
    private readonly CineVerseDbContext _context;

    public PaymentRepository(CineVerseDbContext context):base(context)
    {
        _context = context;
    }

    public async Task<Payment?> GetByProviderPaymentIntentIdAsync(string providerPaymentIntentId, CancellationToken cancellationToken)
    {
        return await _context.Payments
            .FirstOrDefaultAsync(x => x.ProviderPaymentIntentId == providerPaymentIntentId, cancellationToken);
    }

    public async Task<Payment?> GetLatestBySeatHoldIdAsync(int seatHoldId, CancellationToken cancellationToken)
    {
        return await _context.Payments
            .Where(x => x.SeatHoldId == seatHoldId)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsSucceededPaymentBySeatHoldIdAsync(int seatHoldId, CancellationToken cancellationToken)
    {
        return await _context.Payments
            .AnyAsync(x =>
                x.SeatHoldId == seatHoldId &&
                x.Status == PaymentStatus.Succeeded,
                cancellationToken);
    }

    public async Task<bool> ExistsPendingPaymentBySeatHoldIdAsync(int seatHoldId, CancellationToken cancellationToken)
    {
        return await _context.Payments
            .AnyAsync(x =>
                x.SeatHoldId == seatHoldId &&
                x.Status == PaymentStatus.Pending,
                cancellationToken);
    }
    public async Task<Payment?> GetBySeatHoldIdAsync(int seatHoldId, CancellationToken cancellationToken)
    {
        return await _context.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SeatHoldId == seatHoldId, cancellationToken);
    }
    public async Task<(List<Payment> Payments, int TotalCount)> GetByUserIdAsync(
     string userId,
     PaymentStatus? status,
     int pageNumber,
     int pageSize,
     CancellationToken cancellationToken)
    {
        var query = _context.Payments
            .AsNoTracking()
            .Include(x => x.SeatHold)
            .Where(x => x.SeatHold.UserId == userId);

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var payments = await query
            .OrderByDescending(x => x.PaidAtUtc ?? x.PaidAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (payments, totalCount);
    }
    public async Task<Payment?> GetPendingBySeatHoldIdAsync(
    int seatHoldId,
    CancellationToken cancellationToken)
    {
        return await _context.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.SeatHoldId == seatHoldId &&
                x.Status == PaymentStatus.Pending,
                cancellationToken);
    }
    public async Task<Payment?> GetSucceededByScreeningAndSeatAsync(
    int screeningId,
    int seatId,
    CancellationToken cancellationToken)
    {
        return await _context.Payments
            .Include(x => x.SeatHold)
            .FirstOrDefaultAsync(
                x => x.Status == PaymentStatus.Succeeded &&
                     x.SeatHold.ScreeningId == screeningId &&
                     x.SeatHold.SeatId == seatId,
                cancellationToken);
    }
    public async Task<(List<Payment> Payments, int TotalCount)> GetPagedAsync(PaymentStatus? status,PaymentProvider? provider,string? userId,
    DateTime? fromDateUtc, DateTime? toDateUtc,int pageNumber,int pageSize,CancellationToken cancellationToken)
    {
        var query = _context.Payments
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        if (provider.HasValue)
            query = query.Where(x => x.Provider == provider.Value);

        if (!string.IsNullOrWhiteSpace(userId))
            query = query.Where(x => x.UserId == userId);

        if (fromDateUtc.HasValue)
            query = query.Where(x => x.PaidAtUtc >= fromDateUtc.Value);

        if (toDateUtc.HasValue)
            query = query.Where(x => x.PaidAtUtc <= toDateUtc.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var payments = await query
            .OrderByDescending(x => x.PaidAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (payments, totalCount);
    }
    public async Task<(List<Payment> Payments, int TotalCount)> GetRefundHistoryAsync(
    PaymentProvider? provider,
    string? userId,
    DateTime? fromDateUtc,
    DateTime? toDateUtc,
    int pageNumber,
    int pageSize,
    CancellationToken cancellationToken)
    {
        var query = _context.Payments
            .AsNoTracking()
            .Where(x => x.Status == PaymentStatus.Refunded);

        if (provider.HasValue)
            query = query.Where(x => x.Provider == provider.Value);

        if (!string.IsNullOrWhiteSpace(userId))
            query = query.Where(x => x.UserId == userId);

        if (fromDateUtc.HasValue)
            query = query.Where(x => x.RefundedAtUtc >= fromDateUtc.Value);

        if (toDateUtc.HasValue)
            query = query.Where(x => x.RefundedAtUtc <= toDateUtc.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var payments = await query
            .OrderByDescending(x => x.RefundedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (payments, totalCount);
    }
}

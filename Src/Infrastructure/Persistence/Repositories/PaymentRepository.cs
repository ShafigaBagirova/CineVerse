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

}

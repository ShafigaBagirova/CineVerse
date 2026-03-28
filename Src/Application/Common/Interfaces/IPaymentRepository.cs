using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Interfaces;

public interface IPaymentRepository:IRepository<Payment,int>
{
    Task<Payment?> GetByProviderPaymentIntentIdAsync(string providerPaymentIntentId, CancellationToken cancellationToken);
    Task<Payment?> GetLatestBySeatHoldIdAsync(int seatHoldId, CancellationToken cancellationToken);
    Task<bool> ExistsSucceededPaymentBySeatHoldIdAsync(int seatHoldId, CancellationToken cancellationToken);
    Task<bool> ExistsPendingPaymentBySeatHoldIdAsync(int seatHoldId, CancellationToken cancellationToken);
    Task<Payment?> GetBySeatHoldIdAsync(int seatHoldId, CancellationToken cancellationToken);
    Task<(List<Payment> Payments, int TotalCount)> GetByUserIdAsync(string userId, PaymentStatus? status,int pageNumber,int pageSize, CancellationToken cancellationToken);
    Task<Payment?> GetPendingBySeatHoldIdAsync(int seatHoldId, CancellationToken cancellationToken);
    Task<Payment?> GetSucceededByScreeningAndSeatAsync( int screeningId,int seatId,CancellationToken cancellationToken);
    Task<(List<Payment> Payments, int TotalCount)> GetPagedAsync(PaymentStatus? status,PaymentProvider? provider,string? userId,DateTime? fromDateUtc,
    DateTime? toDateUtc,int pageNumber,int pageSize,CancellationToken cancellationToken);
    Task<(List<Payment> Payments, int TotalCount)> GetRefundHistoryAsync(PaymentProvider? provider,string? userId,DateTime? fromDateUtc,
    DateTime? toDateUtc,int pageNumber,int pageSize,CancellationToken cancellationToken);
    Task<int> CountByStatusAsync(PaymentStatus status, CancellationToken cancellationToken = default);
    Task<decimal> SumSuccessfulPaymentsAsync(CancellationToken cancellationToken = default);
}

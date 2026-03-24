using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IPaymentRepository:IRepository<Payment,int>
{
    Task<Payment?> GetByProviderPaymentIntentIdAsync(string providerPaymentIntentId, CancellationToken cancellationToken);
    Task<Payment?> GetLatestBySeatHoldIdAsync(int seatHoldId, CancellationToken cancellationToken);
    Task<bool> ExistsSucceededPaymentBySeatHoldIdAsync(int seatHoldId, CancellationToken cancellationToken);
    Task<bool> ExistsPendingPaymentBySeatHoldIdAsync(int seatHoldId, CancellationToken cancellationToken);
}

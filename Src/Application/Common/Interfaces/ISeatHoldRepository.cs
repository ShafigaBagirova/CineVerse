using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Interfaces;

public interface ISeatHoldRepository:IRepository<SeatHold,int>
{
    Task<SeatHold?> GetActiveHoldAsync(int screeningId, int seatId, CancellationToken cancellationToken);
    Task<(IReadOnlyList<SeatHold> Items, int TotalCount)> GetPagedAsync(int? userId,SeatHoldStatus? status,DateTime? expiresBeforeUtc,
    DateTime? expiresAfterUtc,int pageNumber,int pageSize, CancellationToken cancellationToken);
    Task<(IReadOnlyList<SeatHold> Items, int TotalCount)> GetByScreeningAsync(int screeningId,SeatHoldStatus? status,
    DateTime? expiresBeforeUtc,DateTime? expiresAfterUtc,int pageNumber,int pageSize,CancellationToken cancellationToken);
}

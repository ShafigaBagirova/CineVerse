using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Interfaces;

public interface ITicketRepository: IRepository<Ticket,int>
{
    Task<bool> ExistsByScreeningAndSeatAsync(int screeningId, int seatId, CancellationToken cancellationToken);
    Task<(IReadOnlyList<Ticket> Items, int TotalCount)> GetMyTicketsAsync(string userId,TicketStatus? status, DateTime? purchasedAfterUtc,
    DateTime? purchasedBeforeUtc,int pageNumber, int pageSize,CancellationToken cancellationToken);
    Task<Ticket?> GetDetailedByIdAsync(int id, CancellationToken cancellationToken);
    Task<(IReadOnlyList<Ticket> Items, int TotalCount)> GetByScreeningAsync(int screeningId,TicketStatus? status,
    DateTime? purchasedAfterUtc,DateTime? purchasedBeforeUtc,int pageNumber,int pageSize,
    CancellationToken cancellationToken);
    Task<List<int>> GetSoldSeatIdsByScreeningAsync(int screeningId, CancellationToken cancellationToken);
}


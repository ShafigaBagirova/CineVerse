using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ISeatRepository:IRepository<Seat,int>
{
    Task<bool> ExistsAsync(int hallId, string row, int number, int excludeSeatId, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(int hallId, string row, int number, CancellationToken cancellationToken);
    IQueryable<Seat> GetAll();
    Task<List<Seat>> GetActiveByHallIdAsync(int hallId, CancellationToken cancellationToken);

}

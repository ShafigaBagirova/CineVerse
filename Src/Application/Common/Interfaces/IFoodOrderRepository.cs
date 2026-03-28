using Application.FoodOrders.Dtos;
using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IFoodOrderRepository:IRepository<FoodOrder,int>
{
    Task<FoodOrder?> GetByIdWithItemsAsync(int id, CancellationToken cancellationToken);
    Task<FoodOrder?> GetPendingBySeatHoldIdAsync(int seatHoldId,CancellationToken cancellationToken);

    Task<List<FoodOrder>> GetByUserIdAsync(string userId, CancellationToken cancellationToken);

    Task<List<FoodOrder>> GetByScreeningIdAsync(int screeningId,CancellationToken cancellationToken);
    Task<bool> ExistsPendingBySeatHoldIdAsync(int seatHoldId,CancellationToken cancellationToken);

    Task<List<FoodOrder>> GetPendingByScreeningIdAsync(int screeningId,CancellationToken cancellationToken);
    Task<IQueryable<FoodOrder>> GetQueryableAsync();
    Task<List<FoodOrder>> ToListAsync(IQueryable<FoodOrder> query, CancellationToken cancellationToken = default);

    Task<List<OrdersByDayResponse>> GetOrdersByDayAsync(int? cinemaId, int? screeningId, DateTime? from, DateTime? to,CancellationToken cancellationToken = default);
}

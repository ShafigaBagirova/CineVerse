using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IFoodOrderItemRepository:IRepository<FoodOrderItem,int>
{
    Task AddRangeAsync(List<FoodOrderItem> items, CancellationToken cancellationToken);
}

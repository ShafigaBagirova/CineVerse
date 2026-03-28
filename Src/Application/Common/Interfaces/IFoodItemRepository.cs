using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IFoodItemRepository:IRepository<FoodItem,int>
{
    Task<List<FoodItem>> GetAllActiveAsync(CancellationToken cancellationToken);
    Task<List<FoodItem>> GetByIdsAsync(List<int> ids,CancellationToken cancellationToken);
    Task<IQueryable<FoodItem>> GetQueryableAsync();
    Task<List<FoodItem>> ToListAsync(IQueryable<FoodItem> query, CancellationToken cancellationToken = default);
}

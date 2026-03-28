using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IFoodCategoryRepository:IRepository<FoodCategory,int>
{
    Task<List<FoodCategory>> GetAllActiveWithItemsAsync(CancellationToken cancellationToken);
}

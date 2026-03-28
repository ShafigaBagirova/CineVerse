using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IFoodCategoryRepository:IRepository<FoodCategory,int>
{
    Task<List<FoodCategory>> GetAllActiveWithItemsAsync(CancellationToken cancellationToken);
    Task<IQueryable<FoodCategory>> GetQueryableAsync();
    Task<List<FoodCategory>> ToListAsync(IQueryable<FoodCategory> query, CancellationToken cancellationToken = default);
    Task<List<FoodCategory>> GetCategoriesWithItemsAsync(int? cinemaId,bool? isActive, string? search,
    CancellationToken cancellationToken = default);
}

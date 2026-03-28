using Application.FoodItems.Dtos;

namespace Application.FoodCategories.Dtos;

public class FoodCategoryWithItemsResponse
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public List<FoodItemResponse> Items { get; set; } = new();
}
namespace Application.FoodCategories.Dtos;

public sealed class CreateFoodCategoryRequest
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
}
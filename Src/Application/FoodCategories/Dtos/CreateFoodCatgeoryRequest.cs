namespace Application.FoodCategories.Dtos;

public sealed class CreateFoodCategoryRequest
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
    public int CinemaId { get; set; }
}
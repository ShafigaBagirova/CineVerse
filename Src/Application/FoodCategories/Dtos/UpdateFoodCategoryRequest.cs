namespace Application.FoodCategories.Dtos;

public sealed class UpdateFoodCategoryRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public int? DisplayOrder { get; set; }
    public bool? IsActive { get; set; }
    public int? CinemaId { get; set; }
}
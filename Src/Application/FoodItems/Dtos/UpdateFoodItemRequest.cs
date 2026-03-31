using Microsoft.AspNetCore.Http;

namespace Application.FoodItems.Dtos;

public sealed class UpdateFoodItemRequest
{
    public string? Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public bool? IsAvailable { get; set; }
    public bool? IsActive { get; set; }
    public int? FoodCategoryId { get; set; }
}
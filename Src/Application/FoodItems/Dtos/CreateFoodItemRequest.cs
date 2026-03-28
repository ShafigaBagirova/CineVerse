using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;

namespace Application.FoodItems.Dtos;

public sealed class CreateFoodItemRequest
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public IFormFile? Image { get; set; }
    public bool IsAvailable { get; set; } = true;
    public int FoodCategoryId { get; set; }
}
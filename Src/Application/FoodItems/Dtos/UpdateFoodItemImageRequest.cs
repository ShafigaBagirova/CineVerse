using Microsoft.AspNetCore.Http;

namespace Application.FoodItems.Dtos;

public sealed class UpdateFoodItemImageRequest
{
    public IFormFile Image { get; set; } = default!;
}
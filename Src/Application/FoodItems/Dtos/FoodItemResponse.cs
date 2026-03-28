using Microsoft.AspNetCore.Http;

namespace Application.FoodItems.Dtos;

public class FoodItemResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? Image { get; set; }
    public bool IsAvailable { get; set; }
    public int FoodCategoryId { get; set; }
    public string FoodCategoryName { get; set; } = default!;
    public int CinemaId { get; set; }

}
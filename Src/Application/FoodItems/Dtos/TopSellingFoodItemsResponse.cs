namespace Application.FoodItems.Dtos;

public class TopSellingFoodItemResponse
{
    public int FoodItemId { get; set; }
    public string FoodItemName { get; set; } = default!;
    public int TotalQuantity { get; set; }
}
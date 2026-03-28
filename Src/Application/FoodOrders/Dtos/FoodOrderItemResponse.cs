namespace Application.FoodOrders.Dtos;

public class FoodOrderItemResponse
{
    public int FoodItemId { get; set; }
    public string FoodItemName { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
namespace Application.FoodOrders.Dtos;

public sealed class FoodOrderItemRequest
{
    public int FoodItemId { get; set; }
    public int Quantity { get; set; }
}
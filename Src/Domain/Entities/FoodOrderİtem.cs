namespace Domain.Entities;

public class FoodOrderItem : BaseEntity<int>
{
    public int FoodOrderId { get; set; }
    public FoodOrder FoodOrder { get; set; } = default!;
    public int FoodItemId { get; set; }
    public FoodItem FoodItem { get; set; } = default!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
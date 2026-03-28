namespace Domain.Entities;

public class FoodItem : BaseEntity<int>
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImageObjectKey { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public int FoodCategoryId { get; set; }
    public int CinemaId { get; set; }   
    public Cinema Cinema { get; set; } = default!;
    public FoodCategory FoodCategory { get; set; } = default!;
    public ICollection<FoodOrderItem> FoodOrderItems { get; set; } = new List<FoodOrderItem>();
}

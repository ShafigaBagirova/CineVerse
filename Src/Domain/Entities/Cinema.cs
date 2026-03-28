namespace Domain.Entities;

public class Cinema:BaseEntity<int>
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string Address { get; set; } = default!;
    public string City { get; set; } = default!;
    public string Country { get; set; } = default!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<Hall> Halls { get; set; } = new List<Hall>();
    public ICollection<FoodCategory> FoodCategories { get; set; } = new List<FoodCategory>();
    public ICollection<FoodItem> FoodItems { get; set; } = new List<FoodItem>();
    public ICollection<FoodOrder> FoodOrders { get; set; } = new List<FoodOrder>();

}

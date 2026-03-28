namespace Domain.Entities;

public class FoodCategory : BaseEntity<int>
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    public ICollection<FoodItem> FoodItems { get; set; } = new List<FoodItem>();
}
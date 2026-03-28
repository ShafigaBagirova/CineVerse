namespace Application.FoodCategories.Dtos;

public class FoodCategoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int CinemaId { get; set; }
    public bool IsActive { get; set; }
}
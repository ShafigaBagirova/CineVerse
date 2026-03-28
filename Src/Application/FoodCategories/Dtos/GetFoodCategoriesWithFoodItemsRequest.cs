namespace Application.FoodCategories.Dtos;

public class GetFoodCategoriesWithItemsRequest
{
    public int? CinemaId { get; set; }
    public bool? IsActive { get; set; }
    public string? Search { get; set; }
    public bool OnlyAvailableItems { get; set; } = false;
    public string? SortBy { get; set; }
}
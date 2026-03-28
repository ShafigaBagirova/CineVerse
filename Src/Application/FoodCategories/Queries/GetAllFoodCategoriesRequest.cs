namespace Application.FoodCategories.Queries;

public class GetAllFoodCategoriesRequest
{
    public int? CinemaId { get; set; }
    public bool? IsActive { get; set; }
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
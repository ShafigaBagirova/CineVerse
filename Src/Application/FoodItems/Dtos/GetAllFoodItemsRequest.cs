namespace Application.FoodItems.Dtos;

public class GetAllFoodItemsRequest
{
    public int? CinemaId { get; set; }
    public int? FoodCategoryId { get; set; }
    public bool? IsAvailable { get; set; }
    public string? Search { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
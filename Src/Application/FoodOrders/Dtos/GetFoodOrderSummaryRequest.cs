namespace Application.FoodOrders.Dtos;

public class GetFoodOrderSummaryRequest
{
    public int? CinemaId { get; set; }
    public int? ScreeningId { get; set; }

    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
}
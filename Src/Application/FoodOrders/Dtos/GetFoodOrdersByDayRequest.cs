namespace Application.FoodOrders.Dtos;

public class GetFoodOrdersByDayRequest
{
    public int? CinemaId { get; set; }
    public int? ScreeningId { get; set; }

    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}
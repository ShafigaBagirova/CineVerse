using Domain.Enums;

namespace Application.FoodOrders.Dtos;

public class GetMyFoodOrdersRequest
{
    public int? SeatHoldId { get; set; }
    public int? ScreeningId { get; set; }
    public int? SeatId { get; set; }
    public int? CinemaId { get; set; }
    public FoodOrderStatus? Status { get; set; }
    public DeliveryType? DeliveryType { get; set; }

    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
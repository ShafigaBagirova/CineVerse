using Domain.Enums;

namespace Application.FoodOrders.Dtos;

public class FoodOrderResponse
{
    public int Id { get; set; }
    public string UserId { get; set; } = default!;
    public int? TicketId { get; set; }
    public decimal TotalAmount { get; set; }
    public FoodOrderStatus Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public List<FoodOrderItemResponse> Items { get; set; } = new();
}
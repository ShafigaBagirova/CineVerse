using Domain.Enums;

namespace Application.FoodOrders.Dtos;

public sealed class UpdateFoodOrderDraftRequest
{
    public List<FoodOrderItemRequest>? Items { get; set; } = [];
    public DeliveryType DeliveryType { get; set; } = DeliveryType.SeatDelivery;
    public string? Note { get; set; }
}
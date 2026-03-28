namespace Application.FoodOrders.Dtos;

public class FoodOrderSummaryResponse
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }

    public int PendingOrders { get; set; }
    public int ConfirmedOrders { get; set; }
    public int DeliveredOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int RefundedOrders { get; set; }
}
namespace Application.FoodOrders.Dtos;

public class OrdersByDayResponse
{
    public DateTime Date { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalRevenue { get; set; }
}
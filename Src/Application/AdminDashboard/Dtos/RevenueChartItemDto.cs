namespace Application.AdminDashboard.Dtos;

public sealed class RevenueChartItemDto
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int TicketsSold { get; set; }
}
namespace AdminDashboardMvc.Models;

public class RecentPaymentResponse
{
    public int PaymentId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
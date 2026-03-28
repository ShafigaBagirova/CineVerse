using Domain.Enums;

namespace Application.AdminDashboard.Dtos;

public sealed class RecentPaymentDto
{
    public int PaymentId { get; set; }
    public string UserId { get; set; } = default!;
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = default!;
    public DateTime? CreatedAt { get; set; }
}
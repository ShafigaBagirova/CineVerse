using Domain.Enums;

namespace Application.Payments.Dtos;

public sealed class GetAllPaymentsRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public PaymentStatus? Status { get; set; }
    public PaymentProvider? Provider { get; set; }

    public string? UserId { get; set; }

    public DateTime? FromDateUtc { get; set; }
    public DateTime? ToDateUtc { get; set; }
}

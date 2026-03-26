using Domain.Enums;

namespace Application.Payments.Dtos;

public sealed class GetMyPaymentsRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public PaymentStatus? Status { get; set; }
}
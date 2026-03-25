using Domain.Enums;

namespace Application.Tickets.Dtos;

public sealed class GetMyTicketsRequest
{
    public TicketStatus? Status { get; set; }
    public DateTime? PurchasedAfterUtc { get; set; }
    public DateTime? PurchasedBeforeUtc { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
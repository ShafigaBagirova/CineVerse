using Application.Common.Responses;
using Application.Tickets.Queries;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Application.Tickets.Dtos;

public sealed class GetTicketsByScreeningRequest
{
    public TicketStatus? Status { get; set; }
    public DateTime? PurchasedAfterUtc { get; set; }
    public DateTime? PurchasedBeforeUtc { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
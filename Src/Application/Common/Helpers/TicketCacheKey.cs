namespace Application.Common.Helpers;

public static class TicketCacheKey
{
    public const string GetAllTickets = "tickets:getall";
    public const string GetTicketsByScreeningPrefix = "tickets:screening:";
    public const string GetTicketsByUserPrefix = "tickets:user:";
    public const string GetMyTicketsPrefix = "tickets:my:";
    public const string GetTicketByIdPrefix = "ticket:id:";
}
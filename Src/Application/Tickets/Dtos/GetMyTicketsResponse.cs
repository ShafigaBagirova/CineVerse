using Domain.Enums;

namespace Application.Tickets.Dtos;

public sealed class GetMyTicketsResponse
{
    public int Id { get; set; }
    public int ScreeningId { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = default!;
    public int CinemaId { get; set; }
    public string CinemaName { get; set; } = default!;
    public int HallId { get; set; }
    public string HallName { get; set; } = default!;
    public int SeatId { get; set; }
    public string SeatRow { get; set; } = default!;
    public int SeatNumber { get; set; }
    public DateTime ScreeningStartTime { get; set; }
    public DateTime? ScreeningEndTime { get; set; }

    public decimal Price { get; set; }
    public string Status { get; set; } = default!;
    public DateTime PurchasedAt {  get; set; }
}
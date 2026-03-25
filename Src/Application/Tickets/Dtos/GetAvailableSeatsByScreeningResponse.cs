namespace Application.Tickets.Dtos;

public sealed class GetAvailableSeatsByScreeningResponse
{
    public int SeatId { get; set; }
    public string Row { get; set; } = default!;
    public int Number { get; set; }
}
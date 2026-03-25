using Application.Common.Responses;
using MediatR;

namespace Application.Tickets.Dtos;

public sealed class GetOccupiedSeatsByScreeningResponse
{
    public int SeatId { get; set; }
    public string Row { get; set; } = default!;
    public int Number { get; set; }
    public string OccupancyType { get; set; } = default!;
}

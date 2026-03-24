using Domain.Enums;

namespace Application.SeatHolds.Dtos;

public sealed class GetAllSeatHoldsRequest
{
    public string? UserId { get; set; }
    public SeatHoldStatus? Status { get; set; }
    public DateTime? ExpiresBeforeUtc { get; set; }
    public DateTime? ExpiresAfterUtc { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

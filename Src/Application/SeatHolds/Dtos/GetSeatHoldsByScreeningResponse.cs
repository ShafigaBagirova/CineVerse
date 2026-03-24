namespace Application.SeatHolds.Dtos;

public sealed class GetSeatHoldsByScreeningResponse
{
    public int Id { get; set; }
    public int ScreeningId { get; set; }
    public int SeatId { get; set; }
    public string UserId { get; set; }= default!;
    public DateTime ExpiresAtUtc { get; set; }
    public string Status { get; set; } = default!;
}
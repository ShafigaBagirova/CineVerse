namespace Application.SeatHolds.Dtos;

public class GetAllSeatHoldsResponse
{
    public int Id { get; set; }
    public int ScreeningId { get; set; }
    public int SeatId { get; set; }
    public string UserId { get; set; }=null!;   
    public DateTime ExpiresAtUtc { get; set; }
    public string Status { get; set; } = default!;
}

namespace Application.Common.Dtos;

public sealed class PaymentSuccessNotificationDto
{
    public string MovieTitle { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public string CinemaName { get; set; } = string.Empty;
    public string HallName { get; set; } = string.Empty;
    public string SeatRow { get; set; } = string.Empty;
    public int SeatNumber { get; set; }
}

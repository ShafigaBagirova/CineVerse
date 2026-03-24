using Domain.Enums;

namespace Domain.Entities;

public class Screening: BaseEntity<int>
{
    public int MovieId { get; set; }
    public int HallId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal Price { get; set; }
    public string Language { get; set; } = default!;
    public string? SubtitleLanguage { get; set; }
    public ScreeningFormat Format { get; set; }
    public ScreeningStatus Status { get; set; } = ScreeningStatus.Scheduled;
    public bool IsActive { get; set; } = true;
    public Movie Movie { get; set; } = default!;
    public Hall Hall { get; set; } = default!;
    public ICollection<SeatHold> SeatHolds { get; set; } = new List<SeatHold>();
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}

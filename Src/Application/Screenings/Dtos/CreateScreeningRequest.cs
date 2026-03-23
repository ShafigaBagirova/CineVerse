using Domain.Enums;

namespace Application.Screenings.Dtos;

public class CreateScreeningRequest
{
    public int MovieId { get; set; }
    public int HallId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public decimal Price { get; set; }
    public string Language { get; set; } = default!;
    public string? SubtitleLanguage { get; set; }
    public ScreeningFormat Format { get; set; }
}

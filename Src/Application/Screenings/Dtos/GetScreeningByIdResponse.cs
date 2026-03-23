using Domain.Enums;

namespace Application.Screenings.Dtos;

public class GetScreeningByIdResponse
{
    public int Id { get; set; }

    public int MovieId { get; set; }
    public string MovieTitle { get; set; } = default!;

    public int HallId { get; set; }
    public string HallName { get; set; } = default!;

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public decimal Price { get; set; }

    public string Language { get; set; } = default!;
    public string? SubtitleLanguage { get; set; }

    public ScreeningFormat Format { get; set; }
    public ScreeningStatus Status { get; set; }

    public bool IsActive { get; set; }
}

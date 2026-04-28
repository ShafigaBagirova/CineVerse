namespace Application.Screenings.Dtos;

public class GetAllScreeningsResponse
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

    public string Format { get; set; } = default!;

    public string Status { get; set; } = default!;
    public bool IsActive { get; set; }
}
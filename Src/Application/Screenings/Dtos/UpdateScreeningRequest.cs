using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Application.Screenings.Dtos;

public class UpdateScreeningRequest
{
    public int? MovieId { get; set; }
    public int? HallId { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal? Price { get; set; }
    public string? Language { get; set; }
    public string? SubtitleLanguage { get; set; }
    public ScreeningFormat? Format { get; set; }
    public ScreeningStatus? Status { get; set; }
    public bool? IsActive { get; set; }
}

namespace Domain.Entities;

public class MovieVideo : BaseAuditableEntity
{
    public int MovieId { get; set; }
    public Movie Movie { get; set; } = null!;
    public string VideoKey { get; set; } = null!;
    public string Site { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsOfficial { get; set; }
}
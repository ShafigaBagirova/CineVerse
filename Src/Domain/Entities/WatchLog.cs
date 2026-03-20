namespace Domain.Entities;

public class WatchLog:BaseAuditableEntity
{
    public string UserId { get; set; } = default!;
    public int MovieId { get; set; }
    public Movie Movie { get; set; } = default!;
}

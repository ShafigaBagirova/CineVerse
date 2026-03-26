namespace Domain.Entities;

public class ProcessedWebhookEvent:BaseEntity<int>
{
    public string EventId { get; set; } = default!;
    public DateTime ProcessedAtUtc { get; set; }
}
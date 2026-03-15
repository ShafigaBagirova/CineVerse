namespace Domain.Entities;

public abstract class BaseAuditableEntity:BaseEntity<int>
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

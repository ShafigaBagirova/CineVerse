using Domain.Enums;

namespace Domain.Entities;

public sealed class Notification : BaseAuditableEntity
{
    public string UserId { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public bool IsRead { get; set; }
    public InAppNotificationType Type { get; set; }
}
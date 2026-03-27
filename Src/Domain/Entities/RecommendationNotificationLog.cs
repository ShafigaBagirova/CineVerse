using Domain.Enums;

namespace Domain.Entities;

public sealed class RecommendationNotificationLog : BaseAuditableEntity
{
    public string UserId { get; set; } = default!;
    public RecommendationTargetType TargetType { get; set; }
    public string TargetKey { get; set; }
}
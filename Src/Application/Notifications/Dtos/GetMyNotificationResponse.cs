using Domain.Enums;

namespace Application.Notifications.Dtos;

public sealed class GetMyNotificationsResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public bool IsRead { get; set; }
    public InAppNotificationType Type { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
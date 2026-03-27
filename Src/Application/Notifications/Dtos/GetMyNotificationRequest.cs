namespace Application.Notifications.Dtos;

public sealed class GetMyNotificationsRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
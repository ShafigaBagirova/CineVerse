using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Notifications.Commands;

public sealed class MarkNotificationAsReadCommandHandler
    : IRequestHandler<MarkNotificationAsReadCommand, BaseResponse>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<MarkNotificationAsReadCommandHandler> _logger;

    public MarkNotificationAsReadCommandHandler(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService,
        ILogger<MarkNotificationAsReadCommandHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(
        MarkNotificationAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
            return BaseResponse.Fail("Authenticated user not found.");

        var notification = await _notificationRepository.GetByIdAsync(request.Id, cancellationToken);

        if (notification is null)
            return BaseResponse.Fail("Notification not found.");

        if (notification.UserId != userId)
            return BaseResponse.Fail("You are not allowed to modify this notification.");

        if (notification.IsRead)
            return BaseResponse.Ok("Notification is already marked as read.");

        notification.IsRead = true;

        await _notificationRepository.UpdateAsync(notification, cancellationToken);
        await _notificationRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Notification marked as read. NotificationId: {NotificationId}, UserId: {UserId}",
            notification.Id,
            userId);

        return BaseResponse.Ok("Notification marked as read.");
    }
}

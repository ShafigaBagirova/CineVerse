using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Notifications.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Notifications.Queries;

public sealed class GetUnreadNotificationCountQueryHandler
    : IRequestHandler<GetUnreadNotificationCountQuery, BaseResponse<GetUnreadNotificationCountResponse>>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<GetUnreadNotificationCountQueryHandler> _logger;

    public GetUnreadNotificationCountQueryHandler(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService,
        ILogger<GetUnreadNotificationCountQueryHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<BaseResponse<GetUnreadNotificationCountResponse>> Handle(
        GetUnreadNotificationCountQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
            return BaseResponse<GetUnreadNotificationCountResponse>.Fail("Authenticated user not found.");

        var unreadCount = await _notificationRepository.GetUnreadCountAsync(userId, cancellationToken);

        var response = new GetUnreadNotificationCountResponse
        {
            UnreadCount = unreadCount
        };

        _logger.LogInformation(
            "Unread notification count retrieved successfully. UserId: {UserId}, UnreadCount: {UnreadCount}",
            userId,
            unreadCount);

        return BaseResponse<GetUnreadNotificationCountResponse>.Ok(
            response,
            "Unread notification count retrieved successfully.");
    }
}
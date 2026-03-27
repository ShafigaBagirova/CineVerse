using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Notifications.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Notifications.Queries;

public sealed class GetMyNotificationsQueryHandler
    : IRequestHandler<GetMyNotificationsQuery, BaseResponse<PaginatedResponse<GetMyNotificationsResponse>>>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILogger<GetMyNotificationsQueryHandler> _logger;

    public GetMyNotificationsQueryHandler(
        INotificationRepository notificationRepository,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<GetMyNotificationsQueryHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseResponse<PaginatedResponse<GetMyNotificationsResponse>>> Handle(
        GetMyNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
            return BaseResponse<PaginatedResponse<GetMyNotificationsResponse>>.Fail("Authenticated user not found.");

        var notifications = await _notificationRepository.GetByUserIdAsync(
            userId,
            request.Request.PageNumber,
            request.Request.PageSize,
            cancellationToken);

        var totalCount = await _notificationRepository.CountByUserIdAsync(
            userId,
            cancellationToken);

        var response = new PaginatedResponse<GetMyNotificationsResponse>
        {
            Items = _mapper.Map<List<GetMyNotificationsResponse>>(notifications),
            PageNumber = request.Request.PageNumber,
            PageSize = request.Request.PageSize,
            TotalCount = totalCount
        };

        _logger.LogInformation(
            "Notifications retrieved successfully. UserId: {UserId}, TotalCount: {TotalCount}",
            userId,
            totalCount);

        return BaseResponse<PaginatedResponse<GetMyNotificationsResponse>>.Ok(
            response,
            "Notifications retrieved successfully.");
    }
}
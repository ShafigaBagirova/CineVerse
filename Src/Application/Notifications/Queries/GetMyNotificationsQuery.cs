using Application.Common.Responses;
using Application.Notifications.Dtos;
using MediatR;

namespace Application.Notifications.Queries;

public sealed record GetMyNotificationsQuery(GetMyNotificationsRequest Request)
    : IRequest<BaseResponse<PaginatedResponse<GetMyNotificationsResponse>>>;
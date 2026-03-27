using Application.Common.Responses;
using MediatR;

namespace Application.Notifications.Commands;

public sealed record MarkNotificationAsReadCommand(int Id)
    : IRequest<BaseResponse>;
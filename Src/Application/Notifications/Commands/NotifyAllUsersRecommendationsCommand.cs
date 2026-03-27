using Application.Common.Responses;
using MediatR;

namespace Application.Notifications.Commands;

public sealed record NotifyAllUsersRecommendationsCommand : IRequest<BaseResponse>;

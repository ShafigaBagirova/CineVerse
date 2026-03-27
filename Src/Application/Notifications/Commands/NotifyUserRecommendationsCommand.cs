using Application.Common.Responses;
using MediatR;

namespace Application.Notifications.Commands;

public sealed record NotifyUserRecommendationsCommand(string UserId) : IRequest<BaseResponse>;


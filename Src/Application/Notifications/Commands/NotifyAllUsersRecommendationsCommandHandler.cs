using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Notifications.Commands;

public sealed class NotifyAllUsersRecommendationsCommandHandler
    : IRequestHandler<NotifyAllUsersRecommendationsCommand, BaseResponse>
{
    private readonly IUserIdsProvider _userIdsProvider;
    private readonly ISender _sender;
    private readonly ILogger<NotifyAllUsersRecommendationsCommandHandler> _logger;

    public NotifyAllUsersRecommendationsCommandHandler(
        IUserIdsProvider userIdsProvider,
        ISender sender,
        ILogger<NotifyAllUsersRecommendationsCommandHandler> logger)
    {
        _userIdsProvider = userIdsProvider;
        _sender = sender;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(
        NotifyAllUsersRecommendationsCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("NotifyAllUsersRecommendationsCommand started.");

        var userIds = await _userIdsProvider.GetAllUserIdsAsync(cancellationToken);

        if (userIds.Count == 0)
        {
            _logger.LogInformation("NotifyAllUsersRecommendationsCommand skipped. No users found.");
            return BaseResponse.Ok("No users found.");
        }

        foreach (var userId in userIds)
        {
            try
            {
                await _sender.Send(
                    new NotifyUserRecommendationsCommand(userId),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Recommendation notification failed for UserId: {UserId}",
                    userId);
            }
        }

        _logger.LogInformation(
            "NotifyAllUsersRecommendationsCommand completed successfully. UserCount: {UserCount}",
            userIds.Count);

        return BaseResponse.Ok("Recommendation notifications processed for all users.");
    }
}
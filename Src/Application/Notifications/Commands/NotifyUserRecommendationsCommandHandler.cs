using Application.Common.Interfaces;
using Application.Common.Responses;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Notifications.Commands;

public sealed class NotifyUserRecommendationsCommandHandler
    : IRequestHandler<NotifyUserRecommendationsCommand, BaseResponse>
{
    private readonly IRecommendationService _recommendationService;
    private readonly IRecommendationNotificationLogRepository _recommendationNotificationLogRepository;
    private readonly IInAppNotificationService _inAppNotificationService;
    private readonly ILogger<NotifyUserRecommendationsCommandHandler> _logger;

    public NotifyUserRecommendationsCommandHandler(
        IRecommendationService recommendationService,
        IRecommendationNotificationLogRepository recommendationNotificationLogRepository,
        IInAppNotificationService inAppNotificationService,
        ILogger<NotifyUserRecommendationsCommandHandler> logger)
    {
        _recommendationService = recommendationService;
        _recommendationNotificationLogRepository = recommendationNotificationLogRepository;
        _inAppNotificationService = inAppNotificationService;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(
        NotifyUserRecommendationsCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            _logger.LogWarning("NotifyUserRecommendationsCommand failed. UserId is empty.");
            return BaseResponse.Fail("UserId is required.");
        }

        _logger.LogInformation(
            "NotifyUserRecommendationsCommand started. UserId: {UserId}",
            request.UserId);

        var movieSuggestions = await _recommendationService.GetSuggestedMoviesAsync(
            request.UserId,
            1,
            10,
            cancellationToken);

        var userSuggestions = await _recommendationService.GetSuggestedUsersAsync(
            request.UserId,
            1,
            10,
            cancellationToken);

        var logEntities = new List<RecommendationNotificationLog>();

        foreach (var movie in movieSuggestions.Items)
        {
            var targetKey = movie.Id.ToString();

            var alreadySent = await _recommendationNotificationLogRepository.ExistsAsync(
                request.UserId,
                RecommendationTargetType.Movie,
                targetKey,
                cancellationToken);

            if (alreadySent)
                continue;

            await _inAppNotificationService.CreateAsync(
                request.UserId,
                "Movie recommendation",
                $"We found a movie you may like: {movie.Title}.",
                InAppNotificationType.MovieRecommendation,
                cancellationToken);

            logEntities.Add(new RecommendationNotificationLog
            {
                UserId = request.UserId,
                TargetType = RecommendationTargetType.Movie,
                TargetKey = targetKey,
                CreatedAt = DateTime.UtcNow
            });
        }

        foreach (var suggestedUser in userSuggestions.Items)
        {
            var targetKey = suggestedUser.UserId;

            var alreadySent = await _recommendationNotificationLogRepository.ExistsAsync(
                request.UserId,
                RecommendationTargetType.User,
                targetKey,
                cancellationToken);

            if (alreadySent)
                continue;

            await _inAppNotificationService.CreateAsync(
                request.UserId,
                "Taste match found",
                $"We found a user with a similar movie taste to yours: {suggestedUser.UserName}.",
                InAppNotificationType.UserRecommendation,
                cancellationToken);

            logEntities.Add(new RecommendationNotificationLog
            {
                UserId = request.UserId,
                TargetType = RecommendationTargetType.User,
                TargetKey = targetKey,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (logEntities.Count > 0)
        {
            await _recommendationNotificationLogRepository.AddRangeAsync(
                logEntities,
                cancellationToken);

            await _recommendationNotificationLogRepository.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation(
            "NotifyUserRecommendationsCommand completed successfully. UserId: {UserId}, CreatedLogCount: {CreatedLogCount}",
            request.UserId,
            logEntities.Count);

        return BaseResponse.Ok("Recommendation notifications processed successfully.");
    }
}
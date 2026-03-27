using Application.Notifications.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Recommendation;

public sealed class RecommendationRefreshBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RecommendationRefreshBackgroundService> _logger;

    public RecommendationRefreshBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<RecommendationRefreshBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RecommendationRefreshBackgroundService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();

                await sender.Send(
                    new NotifyAllUsersRecommendationsCommand(),
                    stoppingToken);

                _logger.LogInformation("Recommendation refresh completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Recommendation refresh background job failed.");
            }

            await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
        }
    }
}
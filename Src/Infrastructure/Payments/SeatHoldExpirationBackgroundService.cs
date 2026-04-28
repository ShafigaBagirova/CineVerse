using Application.Common.Interfaces;
using Application.Common.Helpers;
using Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Payments;

public class SeatHoldExpirationBackgroundService: BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SeatHoldExpirationBackgroundService> _logger;

    public SeatHoldExpirationBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<SeatHoldExpirationBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SeatHoldExpirationBackgroundService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var seatHoldRepository = scope.ServiceProvider.GetRequiredService<ISeatHoldRepository>();
                var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

                var expiredActiveSeatHolds =
                    await seatHoldRepository.GetExpiredActiveSeatHoldsAsync(stoppingToken);

                if (expiredActiveSeatHolds.Count > 0)
                {
                    foreach (var seatHold in expiredActiveSeatHolds)
                    {
                        seatHold.Status = SeatHoldStatus.Expired;
                    }

                    await seatHoldRepository.UpdateRangeAsync(expiredActiveSeatHolds, stoppingToken);
                    await seatHoldRepository.SaveChangesAsync(stoppingToken);
                    await cacheService.RemoveByPrefixAsync(SeatHoldCacheKeys.GetAllSeatHoldsPrefix);
                    foreach (var seatHold in expiredActiveSeatHolds)
                    {
                        await cacheService.RemoveAsync($"{SeatHoldCacheKeys.GetSeatHoldByIdPrefix}{seatHold.Id}", stoppingToken);
                        await cacheService.RemoveByPrefixAsync($"{SeatHoldCacheKeys.GetSeatHoldsByScreeningPrefix}{seatHold.ScreeningId}");
                    }

                    _logger.LogInformation(
                        "Expired active seat holds updated successfully. Count: {Count}",
                        expiredActiveSeatHolds.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while expiring seat holds.");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }

        _logger.LogInformation("SeatHoldExpirationBackgroundService stopped.");
    }
}

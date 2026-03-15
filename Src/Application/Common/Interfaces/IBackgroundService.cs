namespace Application.Common.Interfaces;

public interface IBackgroundService
{
    Task ExecuteAsync(CancellationToken stoppingToken);

}

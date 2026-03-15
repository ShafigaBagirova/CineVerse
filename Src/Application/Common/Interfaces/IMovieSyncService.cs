namespace Application.Common.Interfaces;

public interface IMovieSyncService
{
    Task SyncNowPlayingAsync(CancellationToken cancellationToken = default);
    Task SyncUpcomingAsync(CancellationToken cancellationToken = default);
}
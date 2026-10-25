namespace Application.Common.Interfaces;

public interface IWatchRoomNotifier
{
    /// <summary>Tells everyone in the room that it was closed and disconnects them from its live state.</summary>
    Task RoomClosedAsync(string roomCode, CancellationToken cancellationToken);
}

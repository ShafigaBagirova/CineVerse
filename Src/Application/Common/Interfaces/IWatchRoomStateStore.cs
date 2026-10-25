namespace Application.Common.Interfaces;

public sealed record WatchRoomPlaybackState(bool IsPlaying, double PositionSeconds, DateTime UpdatedAtUtc);

public interface IWatchRoomStateStore
{
    WatchRoomPlaybackState GetPlayback(string roomCode);
    WatchRoomPlaybackState ApplyAction(string roomCode, string action, double positionSeconds);
    void AddConnection(string roomCode, string connectionId, string userId);
    void RemoveConnection(string roomCode, string connectionId);
    IReadOnlyCollection<string> GetOnlineUserIds(string roomCode);
}

public static class WatchRoomActions
{
    public const string Play = "play";
    public const string Pause = "pause";
    public const string Seek = "seek";

    public static bool IsValid(string? action)
        => action is Play or Pause or Seek;
}

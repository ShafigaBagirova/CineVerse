using Application.Common.Interfaces;

namespace Infrastructure.WatchRooms;

public sealed class InMemoryWatchRoomStateStore : IWatchRoomStateStore
{
    private readonly object _gate = new();
    private readonly Dictionary<string, WatchRoomPlaybackState> _playback = new();
    private readonly Dictionary<string, Dictionary<string, string>> _connections = new();

    public WatchRoomPlaybackState GetPlayback(string roomCode)
    {
        lock (_gate)
        {
            return CurrentLocked(roomCode);
        }
    }

    public WatchRoomPlaybackState ApplyAction(string roomCode, string action, double positionSeconds)
    {
        var position = double.IsFinite(positionSeconds) ? Math.Max(0, positionSeconds) : 0;

        lock (_gate)
        {
            var isPlaying = action switch
            {
                WatchRoomActions.Play => true,
                WatchRoomActions.Pause => false,
                WatchRoomActions.Seek => CurrentLocked(roomCode).IsPlaying,
                _ => throw new ArgumentException($"Unknown watch room action '{action}'.", nameof(action))
            };

            var state = new WatchRoomPlaybackState(isPlaying, position, DateTime.UtcNow);
            _playback[roomCode] = state;
            return state;
        }
    }

    public void AddConnection(string roomCode, string connectionId, string userId)
    {
        lock (_gate)
        {
            if (!_connections.TryGetValue(roomCode, out var room))
                _connections[roomCode] = room = new Dictionary<string, string>();

            room[connectionId] = userId;
        }
    }

    public void RemoveConnection(string roomCode, string connectionId)
    {
        lock (_gate)
        {
            if (!_connections.TryGetValue(roomCode, out var room))
                return;

            room.Remove(connectionId);
            if (room.Count == 0)
                _connections.Remove(roomCode);
        }
    }

    public IReadOnlyCollection<string> GetOnlineUserIds(string roomCode)
    {
        lock (_gate)
        {
            return _connections.TryGetValue(roomCode, out var room)
                ? room.Values.Distinct().ToList()
                : [];
        }
    }

    private WatchRoomPlaybackState CurrentLocked(string roomCode)
    {
        if (!_playback.TryGetValue(roomCode, out var state))
            return new WatchRoomPlaybackState(false, 0, DateTime.UtcNow);

        if (!state.IsPlaying)
            return state;

        var now = DateTime.UtcNow;
        var elapsed = (now - state.UpdatedAtUtc).TotalSeconds;
        return state with { PositionSeconds = state.PositionSeconds + Math.Max(0, elapsed), UpdatedAtUtc = now };
    }
}

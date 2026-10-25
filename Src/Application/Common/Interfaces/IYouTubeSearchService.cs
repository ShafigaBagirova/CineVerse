using Application.WatchRooms.Dtos;

namespace Application.Common.Interfaces;

public interface IYouTubeSearchService
{
    bool IsConfigured { get; }
    Task<List<SearchYouTubeVideosResponse>> SearchAsync(string query, CancellationToken cancellationToken);
}

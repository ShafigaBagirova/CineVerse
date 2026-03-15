using Application.Movies.Dtos;

namespace Application.Common.Interfaces;

public interface IMovieProvider
{
    Task<IReadOnlyList<ExternalMovieDto>> GetNowPlayingAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExternalMovieDto>> GetUpcomingAsync(CancellationToken cancellationToken = default);
}

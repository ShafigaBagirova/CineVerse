using Application.Movies.Dtos;
using Application.MovieVideos.Dtos;

namespace Application.Common.Interfaces;

public interface IMovieProvider
{
    Task<IReadOnlyList<ExternalMovieDto>> GetNowPlayingAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExternalMovieDto>> GetUpcomingAsync(CancellationToken cancellationToken = default);
    Task<List<ExternalMovieDto>> GetMoviesAsync(int page, CancellationToken cancellationToken = default);
    Task<ExternalTrailerDto?> GetTrailerAsync(long tmdbId, CancellationToken cancellationToken = default);
    Task<List<ExternalGenreDto>> GetGenresAsync(CancellationToken cancellationToken);
}

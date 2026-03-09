using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IMoviePosterRepository
{
    Task<List<MoviePoster>> GetByMovieIdAsync(int movieId, CancellationToken ct);

    Task AddAsync(MoviePoster media, CancellationToken ct);

    Task DeleteAsync(MoviePoster media, CancellationToken ct);
}

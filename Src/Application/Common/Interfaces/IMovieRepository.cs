using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IMovieRepository
{
    Task<Movie?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Movie?> GetByTmdbIdAsync(long tmdbId, CancellationToken cancellationToken = default);

    Task<List<Movie>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Movie movie, CancellationToken cancellationToken = default);

    Task UpdateAsync(Movie movie, CancellationToken cancellationToken = default);
}

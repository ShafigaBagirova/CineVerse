using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IMovieGenreRepository
{
    Task<List<MovieGenre>> GetByMovieIdAsync(int movieId, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(int movieId, int genreId, CancellationToken cancellationToken);
    Task AddAsync(MovieGenre movieGenre, CancellationToken cancellationToken);
    Task RemoveRangeAsync(List<MovieGenre> movieGenres, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

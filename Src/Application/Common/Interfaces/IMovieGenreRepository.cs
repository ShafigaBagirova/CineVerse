using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

public interface IMovieGenreRepository
{
    Task<List<MovieGenre>> GetByMovieIdAsync(int movieId, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(int movieId, int genreId, CancellationToken cancellationToken);
    Task AddAsync(MovieGenre movieGenre, CancellationToken cancellationToken);
    Task RemoveRangeAsync(List<MovieGenre> movieGenres, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task<MovieGenre?> GetPrimaryByMovieIdAsync(int movieId, CancellationToken cancellationToken);
    Task<MovieGenre?> GetByIdsAsync(int movieId, int genreId, CancellationToken cancellationToken);
    Task UpdateAsync(MovieGenre movieGenre, CancellationToken cancellationToken);
    Task DeleteAsync(MovieGenre movieGenre, CancellationToken cancellationToken);
}

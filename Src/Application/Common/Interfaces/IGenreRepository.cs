using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IGenreRepository:IRepository<Genre,int>
{
    Task<IReadOnlyDictionary<int, Genre>> GetByTmdbGenreIdsAsync(
        IReadOnlyCollection<int> tmdbGenreIds,
        CancellationToken cancellationToken);

    Task<Genre?> GetByTmdbGenreIdAsync(int tmdbGenreId, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);
    Task<Genre?> GetByNameAsync(string name, CancellationToken cancellationToken);
}

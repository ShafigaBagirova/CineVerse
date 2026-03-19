using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IGenreRepository:IRepository<Genre,int>
{
    Task<Genre?> GetByTmdbGenreIdAsync(int tmdbGenreId, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);
    Task<Genre?> GetByNameAsync(string name, CancellationToken cancellationToken);
}

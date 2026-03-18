using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IGenreRepository:IRepository<Genre,int>
{
    Task<Genre?> GetByTmdbGenreIdAsync(int tmdbGenreId, CancellationToken cancellationToken);
}

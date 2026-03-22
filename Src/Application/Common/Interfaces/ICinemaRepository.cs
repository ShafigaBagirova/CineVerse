using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ICinemaRepository:IRepository<Cinema,int>
{
    Task<(List<Cinema> Items, int TotalCount)> GetPagedActiveAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);
    Task<List<Cinema>> GetByLocationAsync(string? country, string? city, CancellationToken cancellationToken);
}

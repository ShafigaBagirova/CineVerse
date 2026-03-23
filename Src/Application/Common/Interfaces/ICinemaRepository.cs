using Domain.Entities;

namespace Application.Common.Interfaces;

public interface ICinemaRepository:IRepository<Cinema,int>
{
    Task<(List<Cinema> Items, int TotalCount)> GetPagedActiveAsync(
        int pageNumber,
        int pageSize,
        string? country,
        string? city,
        string? search,
        string? sortBy,
        bool desc,
        CancellationToken cancellationToken);
}

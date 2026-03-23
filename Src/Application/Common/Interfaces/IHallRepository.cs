using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IHallRepository:IRepository<Hall,int>
{
    Task<List<Hall>> GetAllActiveAsync(CancellationToken cancellationToken);
    Task<List<Hall>> GetByCinemaIdAsync(int cinemaId, CancellationToken cancellationToken);
    Task<bool> ExistsByNameInCinemaAsync(int cinemaId,string name,CancellationToken cancellationToken);
    Task<bool> ExistsByNameInCinemaAsync(int cinemaId, string name,int excludeHallId, CancellationToken cancellationToken);
    Task<(List<Hall> Items, int TotalCount)> GetPagedAsync(
    int pageNumber,
    int pageSize,
    int? cinemaId,
    string? search,
    string? sortBy,
    bool desc,
    CancellationToken cancellationToken);

    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);
}

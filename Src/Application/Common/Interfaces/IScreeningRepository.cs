using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Interfaces;

public interface IScreeningRepository:IRepository<Screening,int>
{
    Task<bool> HasTimeConflictAsync(int hallId,DateTime startTime,DateTime endTime,
     CancellationToken cancellationToken);
    Task<bool> HasTimeConflictAsync(int hallId, DateTime startTime,DateTime endTime,int excludeScreeningId,
    CancellationToken cancellationToken);
    Task<Screening?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken);
    Task<(List<Screening> Items, int TotalCount)> GetPagedAsync(int pageNumber,int pageSize,int? movieId,int? hallId,
    ScreeningStatus? status,ScreeningFormat? format,bool? isActive,DateTime? dateFrom,DateTime? dateTo,
    CancellationToken cancellationToken);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);
    Task<int> CountActiveAsync(CancellationToken cancellationToken = default);
}

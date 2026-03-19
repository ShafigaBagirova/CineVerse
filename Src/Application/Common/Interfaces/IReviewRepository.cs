using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IReviewRepository:IRepository<Review,int>
{
    Task<Review?> GetByMovieAndUserAsync(int movieId, string userId, CancellationToken cancellationToken);
    Task<List<Review>> GetByMovieIdAsync(int movieId, CancellationToken cancellationToken);
    Task<int> GetCountByMovieIdAsync(int movieId, CancellationToken cancellationToken);
    Task<List<Review>> GetPagedByMovieIdAsync(int movieId,int page,int pageSize,
        CancellationToken cancellationToken);
    Task<int> GetCountByUserIdAsync(string userId, CancellationToken cancellationToken);
    Task<List<Review>> GetPagedByUserIdAsync(string userId,int page,int pageSize,
        CancellationToken cancellationToken);
}

using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IMovieRatingRepository: IRepository<MovieRating,int>
{
    Task<MovieRating?> GetByMovieAndUserAsync(int movieId, string userId, CancellationToken cancellationToken);
    Task<decimal?> GetAverageRatingAsync(int movieId, CancellationToken cancellationToken);
    Task<int> GetRatingsCountAsync(int movieId, CancellationToken cancellationToken);
    Task<int> GetCountByUserIdAsync(string userId, CancellationToken cancellationToken);
    Task<List<MovieRating>> GetPagedByUserIdAsync(string userId,int page,int pageSize,
    CancellationToken cancellationToken);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}

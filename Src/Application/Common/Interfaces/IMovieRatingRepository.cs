using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IMovieRatingRepository: IRepository<MovieRating,int>
{
    Task<MovieRating?> GetByMovieAndUserAsync(int movieId, string userId, CancellationToken cancellationToken);
    Task<decimal> GetAverageRatingAsync(int movieId, CancellationToken cancellationToken);
}

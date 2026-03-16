using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IMovieVideoRepository : IRepository<MovieVideo, int>
{
    Task<MovieVideo?> GetTrailerByMovieIdAsync(int movieId, CancellationToken cancellationToken = default);
}
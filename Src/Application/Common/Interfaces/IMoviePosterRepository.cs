using Domain.Entities;

namespace Application.Common.Interfaces;

public interface IMoviePosterRepository:IRepository<MoviePoster,int>
{
    Task<List<MoviePoster>> GetByMovieIdAsync(int movieId, CancellationToken ct);

}

using Application.Movies.Dtos;
using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Interfaces;

public interface IMovieRepository:IRepository<Movie,int>
{
    Task<Movie?> GetByTmdbIdAsync(long tmdbId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);
    Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<bool> ExistsByTmdbIdAsync(long tmdbId, CancellationToken cancellationToken);
    Task<bool> ExistsByTitleAndReleaseDateAsync(string title, DateOnly? releaseDate, CancellationToken cancellationToken);
    Task<Movie?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<(List<Movie> Items, int TotalCount)> GetPagedAsync(int pageNumber,int pageSize, string? search,int? genreId, string? language,MovieStatus? Status,
     int? year, decimal? minTmdbRating, decimal? maxTmdbRating, decimal? minUserRating, decimal? maxUserRating,string? sortBy,bool desc,
        CancellationToken cancellationToken);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<Movie?> GetMovieWithDetailsByIdAsync(int id, CancellationToken cancellationToken = default);
}

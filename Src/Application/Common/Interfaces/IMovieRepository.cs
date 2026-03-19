using Application.Movies.Dtos;
using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Interfaces;

public interface IMovieRepository:IRepository<Movie,int>
{
    Task<Movie?> GetByTmdbIdAsync(long tmdbId, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<bool> ExistsByTmdbIdAsync(long tmdbId, CancellationToken cancellationToken);
    Task<bool> ExistsByTitleAndReleaseDateAsync(string title, DateOnly? releaseDate, CancellationToken cancellationToken);
    Task<Movie?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<List<Movie>> GetPagedAsync(int pageNumber, int pageSize, MovieSortBy sortBy,
    CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<Movie?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CountByStatusAsync(MovieStatus status, CancellationToken cancellationToken = default);
    Task<int> CountSearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<List<Movie>> GetByStatusPagedAsync(MovieStatus status, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<List<Movie>> SearchPagedAsync(string searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountByLanguageAsync(string language, CancellationToken cancellationToken = default);
    Task<List<Movie>> GetByLanguagePagedAsync(string language,int pageNumber,int pageSize,CancellationToken cancellationToken = default);
    Task<int> CountByYearAsync(int year, CancellationToken cancellationToken = default);
    Task<List<Movie>> GetByYearPagedAsync( int year, int pageNumber,int pageSize,
        CancellationToken cancellationToken = default);
    Task<int> CountByUserRatingRangeAsync(decimal minRating, decimal maxRating, CancellationToken cancellationToken = default);
    Task<List<Movie>> GetByUserRatingRangePagedAsync(
        decimal minRating,
        decimal maxRating,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<int> CountByTmdbRatingRangeAsync(decimal minRating,decimal maxRating,CancellationToken cancellationToken = default);
    Task<List<Movie>> GetByTmdbRatingRangePagedAsync(decimal minRating, decimal maxRating,int pageNumber,int pageSize,
        CancellationToken cancellationToken = default);
    Task<List<Movie>> GetMoviesByGenreAsync(
    int genreId,
    int page,
    int pageSize,
    CancellationToken cancellationToken);

    Task<int> GetMoviesCountByGenreAsync(
        int genreId,
        CancellationToken cancellationToken);
    Task<Movie?> GetByIdWithGenresAsync(int id, CancellationToken cancellationToken);
}

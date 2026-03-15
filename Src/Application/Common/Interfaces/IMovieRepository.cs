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
    Task<List<Movie>> GetPagedWithMediaAsync(int pageNumber, int pageSize, MovieSortBy sortBy,
    CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task<Movie?> GetByIdWithMediaAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CountByStatusAsync(MovieStatus status, CancellationToken cancellationToken = default);
    Task<int> CountSearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<List<Movie>> GetByStatusPagedWithMediaAsync(MovieStatus status, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<List<Movie>> SearchPagedWithMediaAsync(string searchTerm, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Movie?> GetBySlugWithMediaAsync(string slug, CancellationToken cancellationToken = default);
    Task<int> CountByLanguageAsync(string language, CancellationToken cancellationToken = default);
    Task<List<Movie>> GetByLanguagePagedWithMediaAsync(string language,int pageNumber,int pageSize,CancellationToken cancellationToken = default);
    Task<int> CountByYearAsync(int year, CancellationToken cancellationToken = default);
    Task<List<Movie>> GetByYearPagedWithMediaAsync( int year, int pageNumber,int pageSize,
        CancellationToken cancellationToken = default);
    Task<int> CountByUserRatingRangeAsync(decimal minRating, decimal maxRating, CancellationToken cancellationToken = default);
    Task<List<Movie>> GetByUserRatingRangePagedWithMediaAsync(
        decimal minRating,
        decimal maxRating,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}

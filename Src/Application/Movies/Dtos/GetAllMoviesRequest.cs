using Domain.Enums;

namespace Application.Movies.Dtos;

public class GetAllMoviesRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? ActorName { get; set; }
    public string? DirectorName { get; set; }
    public int? GenreId { get; set; }
    public string? Language { get; set; }
    public MovieStatus? Status { get; set; }
    public int? Year { get; set; }
    public decimal? MinTmdbRating { get; set; }
    public decimal? MaxTmdbRating { get; set; }
    public decimal? MinUserRating { get; set; }
    public decimal? MaxUserRating { get; set; }
    public string? SortBy { get; set; }
    public bool Desc { get; set; } = false;
}
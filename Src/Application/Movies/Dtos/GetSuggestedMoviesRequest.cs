namespace Application.Movies.Dtos;

public sealed class GetSuggestedMoviesRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
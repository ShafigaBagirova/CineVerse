namespace Application.Movies.Dtos;

public class GetAllMoviesResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? FirstMediaKey { get; set; }
}
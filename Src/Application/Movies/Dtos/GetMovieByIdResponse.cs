using Application.MoviePost.Dtos;

namespace Application.Movies.Dtos;

public class GetMovieByIdResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; }=null!;
    public string FirstMediaKey { get; set; } = null!;
}
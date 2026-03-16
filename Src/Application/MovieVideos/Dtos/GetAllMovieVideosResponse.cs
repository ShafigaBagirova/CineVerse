namespace Application.MovieVideos.Dtos;

public sealed class GetAllMovieVideosResponse
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string VideoKey { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Name { get; set; } = null!;
}
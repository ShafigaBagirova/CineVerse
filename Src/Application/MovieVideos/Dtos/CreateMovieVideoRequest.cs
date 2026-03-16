namespace Application.MovieVideos.Dtos;

public sealed class CreateMovieVideoRequest
{
    public int MovieId { get; set; }
    public string VideoKey { get; set; } = null!;
    public string Site { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsOfficial { get; set; }
}
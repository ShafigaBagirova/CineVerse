namespace Application.MovieVideos.Dtos;

public sealed class UpdateMovieVideoRequest
{
    public int? MovieId { get; set; }
    public string? VideoKey { get; set; }
    public string? Site { get; set; }
    public string? Type { get; set; }
    public string? Name { get; set; }
    public bool? IsOfficial { get; set; }
}
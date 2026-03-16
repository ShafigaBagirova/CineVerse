namespace Application.MovieVideos.Dtos;

public sealed class ExternalTrailerDto
{
    public string Key { get; set; } = null!;
    public string Site { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsOfficial { get; set; }
}
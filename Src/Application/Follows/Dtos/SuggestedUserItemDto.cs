namespace Application.Follows.Dtos;

public sealed class SuggestedUserItemDto
{
    public string UserId { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public int TasteScore { get; set; }
    public int CommonMoviesCount { get; set; }
}

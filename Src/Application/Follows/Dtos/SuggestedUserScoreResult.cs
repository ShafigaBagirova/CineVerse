namespace Application.Follows.Dtos;

public sealed class SuggestedUserScoreResult
{
    public string UserId { get; set; } = default!;
    public int TasteScore { get; set; }
    public int CommonMoviesCount { get; set; }
}
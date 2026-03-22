namespace Application.Follows.Dtos;

public sealed class FollowInsightsDto
{
    public int FollowersCount { get; set; }
    public int FollowingsCount { get; set; }

    public int MutualFollowersCount { get; set; }
    public int MutualFollowingsCount { get; set; }

    public int SuggestedUsersCount { get; set; }

    public double TasteSimilarityScore { get; set; }
}
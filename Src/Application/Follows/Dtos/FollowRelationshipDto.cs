namespace Application.Follows.Dtos;

public sealed class FollowRelationshipDto
{
    public bool IsFollowing { get; set; }
    public bool IsFollowedBy { get; set; }
    public bool IsMutual { get; set; }
    public bool IsSelf { get; set; }
}
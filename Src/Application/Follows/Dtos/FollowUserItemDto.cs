namespace Application.Follows.Dtos;

public sealed class FollowUserItemDto
{
    public string UserId { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
}
namespace Application.Auth.User.Dtos;

public sealed record UserProfileDto(
    string UserId,
    string UserName,
    string FullName,
    string? AvatarUrl,
    string? Email = null)
{
    public string Id => UserId;
    public string? ProfileImageUrl => AvatarUrl;
}

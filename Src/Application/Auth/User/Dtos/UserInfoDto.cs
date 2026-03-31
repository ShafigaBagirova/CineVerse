namespace Application.Auth.User.Dtos;

public class UserInfoDto
{
    public string Id { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? UserName { get; set; }
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public List<string> Roles { get; set; } = new();
}
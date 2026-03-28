namespace Application.Auth.User.Dtos;

public sealed record UserProfileDto(string UserId,string UserName,string FullName,string? AvatarUrl);

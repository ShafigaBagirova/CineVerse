namespace Application.Auth.Login.Dtos;

public sealed record JwtUserInfoDto(
    string UserId,
    string UserName,
    string Email,
     IEnumerable<string> Roles
);

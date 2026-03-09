using Application.Auth.Login.Dtos;

namespace Application.Auth.Refresh.Queries;
public sealed record RefreshTokenQueryResult(
    string Token,
    string UserId,
    DateTime ExpiresAtUtc,
    JwtUserInfoDto User
);

using Application.Auth.Login.Dtos;

namespace Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    (string AccessToken, DateTime ExpiresAtUtc) GenerateAccessToken(JwtUserInfoDto user, IEnumerable<string> roles);
}
namespace Application.Auth.Login.Dtos;

public class TokenResponse
{
    public string AccessToken { get; init; } = default!;
    public string RefreshToken { get; init; } = default!;
    public DateTime ExpiresAtUtc { get; init; }
}

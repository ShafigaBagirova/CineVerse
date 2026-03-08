namespace Application.Auth.Refresh.Dtos;

public sealed class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = null!;
}
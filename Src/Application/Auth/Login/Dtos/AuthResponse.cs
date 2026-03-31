namespace Application.Auth.Login.Dtos;

public class AuthResponse
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
    public DateTime AccessTokenExpiresAtUtc { get; set; }
    public string UserId { get; set; }= default!;
    public string Email { get; set; } = default!;
    public string FullName { get; set; } = default!;
}

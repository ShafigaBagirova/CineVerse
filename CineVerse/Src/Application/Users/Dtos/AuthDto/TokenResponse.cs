namespace CineVerse.Application.Users.Dtos.AuthDto;

public class TokenResponse
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
    public DateTime ExpiresAtUtc { get; set; }
}

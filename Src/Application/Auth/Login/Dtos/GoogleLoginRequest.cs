namespace Application.Auth.Login.Dtos;

public sealed class GoogleLoginRequest
{
    public string IdToken { get; set; } = default!;
}
namespace Application.Auth.Login.Dtos;

public sealed class LoginRequest
{
    public string Login { get; set; } = null!;  
    public string Password { get; set; } = null!;

}

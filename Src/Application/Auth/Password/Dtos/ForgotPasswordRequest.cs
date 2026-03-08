namespace Application.Auth.Password.Dtos;

public sealed class ForgotPasswordRequest
{
    public string Email { get; set; } = null!;
}
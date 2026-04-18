namespace Application.Auth.Register.Dtos;

public sealed class ResendVerificationCodeRequest
{
    public string Email { get; set; } = default!;
}

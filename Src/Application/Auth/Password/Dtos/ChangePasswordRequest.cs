namespace Application.Auth.Password.Dtos;

public sealed class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}

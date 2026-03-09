namespace Application.Auth.Email.Dtos;

public sealed class ConfirmUpdateEmailRequest
{
    public string NewEmail { get; set; } = null!;
    public string Code { get; set; } = null!;
}

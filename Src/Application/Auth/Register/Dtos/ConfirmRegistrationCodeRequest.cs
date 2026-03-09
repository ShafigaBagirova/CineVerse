namespace Application.Auth.Register.Dtos;

public sealed record ConfirmRegistrationCodeRequest(
    string Email,
    string Code
);

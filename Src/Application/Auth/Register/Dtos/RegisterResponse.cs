namespace Application.Auth.Register.Dtos;

public sealed record RegisterResponse(
    bool IsCodeSent,
    string Message
);
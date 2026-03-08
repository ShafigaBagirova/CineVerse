using MediatR;

namespace Application.Auth.Register.Commands;

public sealed record ConfirmRegisterCommand(
    string Email,
    string Code
) : IRequest<bool>;

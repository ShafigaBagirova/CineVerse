using Application.Auth.Login.Commands;
using FluentValidation;

namespace Application.Validations.Auth;

public class LoginRequestValidator : AbstractValidator<LoginCommand>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.LoginRequest.Login)
            .NotEmpty().WithMessage("Username or email is required.")
            .MaximumLength(256).WithMessage("Login value is too long.");

        RuleFor(x => x.LoginRequest.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Invalid credentials.");
    }
}
using Application.Auth.Login.Dtos;
using FluentValidation;

namespace Application.Validations;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Login)
            .NotEmpty().WithMessage("Username or email is required.")
            .MaximumLength(256).WithMessage("Login value is too long.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Invalid credentials.");
    }
}
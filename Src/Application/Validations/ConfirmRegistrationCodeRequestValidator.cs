using Application.Auth.Register.Dtos;
using FluentValidation;

namespace Application.Validations;

public sealed class ConfirmRegistrationCodeRequestValidator : AbstractValidator<ConfirmRegistrationCodeRequest>
{
    public ConfirmRegistrationCodeRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email format is invalid.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .Length(6).WithMessage("Code must be 6 characters.")
            .Matches("^[0-9]{6}$").WithMessage("Code must contain exactly 6 digits.");
    }
}

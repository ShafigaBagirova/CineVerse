using Application.Auth.Email.Dtos;
using FluentValidation;

namespace Application.Validations.Email;
public sealed class ConfirmEmailChangeRequestValidator : AbstractValidator<ConfirmUpdateEmailRequest>
{
    public ConfirmEmailChangeRequestValidator()
    {
        RuleFor(x => x.NewEmail)
            .NotEmpty().WithMessage("New email is required.")
            .EmailAddress().WithMessage("Email format is invalid.");
        RuleFor(x => x.Code)
                    .NotEmpty().WithMessage("Code is required.")
                    .Length(6).WithMessage("Verification code must be 6 characters.");
    }
}

using Application.Auth.Login.Commands;
using FluentValidation;

namespace Application.Validations.Auth;

public sealed class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
{
    public GoogleLoginCommandValidator()
    {
        RuleFor(x => x.IdToken)
            .NotEmpty().WithMessage("Google id token is required.");
    }
}
using Application.Auth.Email.Dtos;
using FluentValidation;

namespace Application.Validations;

    public sealed class UpdateEmailRequestValidator : AbstractValidator<UpdateEmailRequest>
    {
        public UpdateEmailRequestValidator()
        {
            RuleFor(x => x.NewEmail)
                .NotEmpty().WithMessage("New email is required.")
                .EmailAddress().WithMessage("Email format is invalid.");
        }
    }

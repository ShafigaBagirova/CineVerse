using Application.Cinemas.Commands;
using FluentValidation;

namespace Application.Validations.Cinema;

public class CreateCinemaCommandValidator : AbstractValidator<CreateCinemaCommand>
{
    public CreateCinemaCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Name cannot be empty.")
            .MaximumLength(200);

        RuleFor(x => x.Request.Address)
            .NotEmpty().WithMessage("Address cannot be empty.");

        RuleFor(x => x.Request.City)
            .NotEmpty().WithMessage("City cannot be empty.");

        RuleFor(x => x.Request.Country)
            .NotEmpty().WithMessage("Country cannot be empty.");

        RuleFor(x => x.Request.Email)
            .NotEmpty().WithMessage("Email cannot be empty.")
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Request.Email));
    }
}

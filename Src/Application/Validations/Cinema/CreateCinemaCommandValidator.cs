using Application.Cinemas.Commands;
using FluentValidation;

namespace Application.Validations.Cinema;

public class CreateCinemaCommandValidator : AbstractValidator<CreateCinemaCommand>
{
    public CreateCinemaCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Name boş ola bilməz")
            .MaximumLength(200);

        RuleFor(x => x.Request.Address)
            .NotEmpty().WithMessage("Address boş ola bilməz");

        RuleFor(x => x.Request.City)
            .NotEmpty();

        RuleFor(x => x.Request.Country)
            .NotEmpty();

        RuleFor(x => x.Request.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Request.Email));
    }
}

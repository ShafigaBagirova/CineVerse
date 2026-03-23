using Application.Cinemas.Commands;
using FluentValidation;

namespace Application.Validations.Cinema;

public class UpdateCinemaCommandValidator : AbstractValidator<UpdateCinemaCommand>
{
    public UpdateCinemaCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Cinema Id must be greater than 0.");

        RuleFor(x => x.Request.Name)
            .MaximumLength(200);

        RuleFor(x => x.Request.Description)
            .MaximumLength(1000);

        RuleFor(x => x.Request.Address)
            .MaximumLength(300);

        RuleFor(x => x.Request.Phone)
            .MaximumLength(30);

        RuleFor(x => x.Request.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Request.Email))
            .WithMessage("Email is not in the correct form.");
    }
}
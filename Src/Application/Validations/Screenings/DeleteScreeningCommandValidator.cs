using Application.Screenings.Commands;
using FluentValidation;

namespace Application.Validations.Screenings;

public sealed class DeleteScreeningCommandValidator : AbstractValidator<DeleteScreeningCommand>
{
    public DeleteScreeningCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Screening id must be greater than 0.");
    }
}
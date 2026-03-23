using Application.Halls.Commands;
using FluentValidation;

namespace Application.Validations.Halls;

public class DeleteHallCommandValidator : AbstractValidator<DeleteHallCommand>
{
    public DeleteHallCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Hall id must be greater than 0.");
    }
}
using Application.Cinemas.Commands;
using FluentValidation;

namespace Application.Validations.Cinema;

public class DeleteCinemaCommandValidator : AbstractValidator<DeleteCinemaCommand>
{
    public DeleteCinemaCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Cinema Id must be greater than 0.");
    }
}
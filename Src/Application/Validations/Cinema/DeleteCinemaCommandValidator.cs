using Application.Cinemas.Commands;
using FluentValidation;

namespace Application.Validations.Cinema;

public class DeleteCinemaCommandValidator : AbstractValidator<DeleteCinemaCommand>
{
    public DeleteCinemaCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Cinema Id 0-dan böyük olmalıdır.");
    }
}
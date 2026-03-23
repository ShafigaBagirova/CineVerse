using Application.Seats.Commands;
using FluentValidation;

namespace Application.Validations.Seats;

public sealed class DeleteSeatCommandValidator : AbstractValidator<DeleteSeatCommand>
{
    public DeleteSeatCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Seat id must be greater than 0.");
    }
}
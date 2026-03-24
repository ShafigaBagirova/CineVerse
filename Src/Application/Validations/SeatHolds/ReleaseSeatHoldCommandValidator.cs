using Application.SeatHolds.Commands;
using FluentValidation;

namespace Application.Validations.SeatHolds;

public sealed class ReleaseSeatHoldCommandValidator : AbstractValidator<ReleaseSeatHoldCommand>
{
    public ReleaseSeatHoldCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Seat hold id must be greater than 0.");
    }
}
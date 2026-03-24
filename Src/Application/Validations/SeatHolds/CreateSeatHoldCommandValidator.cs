using Application.SeatHolds.Commands;
using FluentValidation;

namespace Application.Validations.SeatHolds;

public sealed class CreateSeatHoldCommandValidator : AbstractValidator<CreateSeatHoldCommand>
{
    public CreateSeatHoldCommandValidator()
    {
        RuleFor(x => x.Request.ScreeningId)
            .GreaterThan(0)
            .WithMessage("ScreeningId must be greater than 0.");

        RuleFor(x => x.Request.SeatId)
            .GreaterThan(0)
            .WithMessage("SeatId must be greater than 0.");
    }
}
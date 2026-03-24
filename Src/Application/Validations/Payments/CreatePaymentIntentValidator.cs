using Application.Payments.Commands;
using FluentValidation;

namespace Application.Validations.Payments;

public sealed class CreatePaymentIntentCommandValidator
    : AbstractValidator<CreatePaymentIntentCommand>
{
    public CreatePaymentIntentCommandValidator()
    {
        RuleFor(x => x.Request.SeatHoldId)
            .GreaterThan(0)
            .WithMessage("Seat hold id must be greater than 0.");
    }
}
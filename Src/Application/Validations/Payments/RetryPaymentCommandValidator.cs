using Application.Payments.Commands;
using FluentValidation;

namespace Application.Validations.Payments;

public sealed class RetryPaymentCommandValidator : AbstractValidator<RetryPaymentCommand>
{
    public RetryPaymentCommandValidator()
    {
        RuleFor(x => x.SeatHoldId)
            .NotEmpty()
            .WithMessage("SeatHoldId is required.");
    }
}
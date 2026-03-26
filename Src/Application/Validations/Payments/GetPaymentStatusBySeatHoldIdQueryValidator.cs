using Application.Payments.Queries;
using FluentValidation;

namespace Application.Validations.Payments;

public sealed class GetPaymentStatusBySeatHoldIdQueryValidator
    : AbstractValidator<GetPaymentStatusBySeatHoldIdQuery>
{
    public GetPaymentStatusBySeatHoldIdQueryValidator()
    {
        RuleFor(x => x.SeatHoldId)
            .NotEmpty()
            .WithMessage("SeatHoldId is required.");
    }
}
using Application.Payments.Queries;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Validations.Payments;

public sealed class GetPaymentByIdQueryValidator : AbstractValidator<GetPaymentByIdQuery>
{
    public GetPaymentByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Payment id must be greater than 0.");
    }
}
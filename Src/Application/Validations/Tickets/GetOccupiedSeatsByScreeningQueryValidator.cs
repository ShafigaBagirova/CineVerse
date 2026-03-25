using Application.Tickets.Queries;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Validations.Tickets;

public sealed class GetOccupiedSeatsByScreeningQueryValidator
    : AbstractValidator<GetOccupiedSeatsByScreeningQuery>
{
    public GetOccupiedSeatsByScreeningQueryValidator()
    {
        RuleFor(x => x.ScreeningId)
            .GreaterThan(0)
            .WithMessage("Screening id must be greater than 0.");
    }
}
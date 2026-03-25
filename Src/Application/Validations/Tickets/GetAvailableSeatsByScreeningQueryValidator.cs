using Application.Tickets.Queries;
using FluentValidation;

namespace Application.Validations.Tickets;

public sealed class GetAvailableSeatsByScreeningQueryValidator
    : AbstractValidator<GetAvailableSeatsByScreeningQuery>
{
    public GetAvailableSeatsByScreeningQueryValidator()
    {
        RuleFor(x => x.ScreeningId)
            .GreaterThan(0)
            .WithMessage("Screening id must be greater than 0.");
    }
}
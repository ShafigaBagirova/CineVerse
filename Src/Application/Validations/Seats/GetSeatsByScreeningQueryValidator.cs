using Application.Seats.Queries;
using FluentValidation;

namespace Application.Validations.Seats;

public sealed class GetSeatsByScreeningQueryValidator
    : AbstractValidator<GetSeatsByScreeningQuery>
{
    public GetSeatsByScreeningQueryValidator()
    {
        RuleFor(x => x.ScreeningId)
            .GreaterThan(0)
            .WithMessage("Screening id must be greater than 0.");
    }
}
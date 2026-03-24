using Application.SeatHolds.Queries;
using FluentValidation;

namespace Application.Validations.SeatHolds;

public sealed class GetSeatHoldByIdQueryValidator : AbstractValidator<GetSeatHoldByIdQuery>
{
    public GetSeatHoldByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Seat hold id must be greater than 0.");
    }
}
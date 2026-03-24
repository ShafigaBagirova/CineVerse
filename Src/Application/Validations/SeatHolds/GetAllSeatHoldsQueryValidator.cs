using Application.SeatHolds.Queries;
using FluentValidation;

namespace Application.Validations.SeatHolds;

public sealed class GetAllSeatHoldsQueryValidator : AbstractValidator<GetAllSeatHoldsQuery>
{
    public GetAllSeatHoldsQueryValidator()
    {
        RuleFor(x => x.Request.PageNumber)
            .GreaterThan(0)
            .WithMessage("PageNumber must be greater than 0.");

        RuleFor(x => x.Request.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("PageSize must be between 1 and 100.");
    }
}
using Application.Seats.Queries;
using FluentValidation;

namespace Application.Validations.Seats;

public sealed class GetAllSeatsQueryValidator : AbstractValidator<GetAllSeatsQuery>
{
    public GetAllSeatsQueryValidator()
    {
        When(x => x.Request.HallId.HasValue, () =>
        {
            RuleFor(x => x.Request.HallId!.Value)
                .GreaterThan(0)
                .WithMessage("Hall id must be greater than 0.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Request.Row), () =>
        {
            RuleFor(x => x.Request.Row!)
                .MaximumLength(10)
                .WithMessage("Row must not exceed 10 characters.");
        });

        When(x => x.Request.Type.HasValue, () =>
        {
            RuleFor(x => x.Request.Type!.Value)
                .IsInEnum()
                .WithMessage("Seat type is invalid.");
        });

        RuleFor(x => x.Request.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.Request.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("Page size must be between 1 and 100.");
    }
}
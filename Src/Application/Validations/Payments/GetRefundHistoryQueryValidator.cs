using Application.Payments.Queries;
using FluentValidation;

namespace Application.Validations.Payments;

public sealed class GetRefundHistoryQueryValidator : AbstractValidator<GetRefundHistoryQuery>
{
    public GetRefundHistoryQueryValidator()
    {
        RuleFor(x => x.Request.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.Request.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.Request.ToDateUtc)
            .GreaterThanOrEqualTo(x => x.Request.FromDateUtc!.Value)
            .When(x => x.Request.FromDateUtc.HasValue && x.Request.ToDateUtc.HasValue)
            .WithMessage("ToDateUtc must be greater than or equal to FromDateUtc.");
    }
}
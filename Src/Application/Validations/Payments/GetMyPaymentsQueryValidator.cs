using Application.Payments.Queries;
using FluentValidation;

namespace Application.Validations.Payments;

public sealed class GetMyPaymentsQueryValidator : AbstractValidator<GetMyPaymentsQuery>
{
    public GetMyPaymentsQueryValidator()
    {

        RuleFor(x => x.Request.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.Request.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.Request.Status)
          .IsInEnum()
          .When(x => x.Request.Status.HasValue)
          .WithMessage("Invalid payment status.");
    }

    private static bool BeValidStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return true;

        return status.Equals("Pending", StringComparison.OrdinalIgnoreCase)
               || status.Equals("Succeeded", StringComparison.OrdinalIgnoreCase)
               || status.Equals("Failed", StringComparison.OrdinalIgnoreCase)
               || status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)
               || status.Equals("Refunded", StringComparison.OrdinalIgnoreCase);
    }
}
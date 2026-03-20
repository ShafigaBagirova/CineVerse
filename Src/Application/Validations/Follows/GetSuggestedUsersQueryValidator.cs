using Application.Follows.Queries;
using FluentValidation;

namespace Application.Validations.Follows;

public sealed class GetSuggestedUsersQueryValidator : AbstractValidator<GetSuggestedUsersQuery>
{
    public GetSuggestedUsersQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(50)
            .WithMessage("Page size must not exceed 50.");
    }
}
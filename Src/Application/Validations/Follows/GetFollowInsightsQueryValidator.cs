using Application.Follows.Queries;
using FluentValidation;

namespace Application.Validations.Follows;

public sealed class GetFollowInsightsQueryValidator : AbstractValidator<GetFollowInsightsQuery>
{
    public GetFollowInsightsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User id is required.")
            .MaximumLength(450);
    }
}
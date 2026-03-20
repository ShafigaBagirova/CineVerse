using Application.Follows.Queries;
using FluentValidation;

namespace Application.Validations.Follows;

public sealed class GetFollowStatusQueryValidator : AbstractValidator<GetFollowStatusQuery>
{
    public GetFollowStatusQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User id is required.")
            .MaximumLength(450)
            .WithMessage("User id must not exceed 450 characters.");
    }
}
using Application.Follows.Commands;
using FluentValidation;

namespace Application.Validations.Follows;

public sealed class UnfollowUserCommandValidator : AbstractValidator<UnfollowUserCommand>
{
    public UnfollowUserCommandValidator()
    {
        RuleFor(x => x.FollowingId)
            .NotEmpty()
            .WithMessage("Following user id is required.")
            .MaximumLength(450)
            .WithMessage("Following user id must not exceed 450 characters.");
    }
}
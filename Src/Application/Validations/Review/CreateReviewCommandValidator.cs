using Application.Reviews.Commands;
using FluentValidation;

namespace Application.Validations.Review;

public sealed class CreateReviewCommandValidator
    : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(x => x.MovieId)
            .GreaterThan(0);

        RuleFor(x => x.Request.Content)
            .NotEmpty()
            .MaximumLength(1000);
    }
}
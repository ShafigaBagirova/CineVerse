using Application.Reviews.Commands;
using FluentValidation;

namespace Application.Validations.Review;

public sealed class DeleteReviewCommandValidator
    : AbstractValidator<DeleteReviewCommand>
{
    public DeleteReviewCommandValidator()
    {
        RuleFor(x => x.MovieId)
            .GreaterThan(0);
    }
}

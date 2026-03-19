using Application.Movies.Commands;
using FluentValidation;

namespace Application.Validations.Movie;

public sealed class DeleteReviewCommandValidator
    : AbstractValidator<DeleteReviewCommand>
{
    public DeleteReviewCommandValidator()
    {
        RuleFor(x => x.MovieId)
            .GreaterThan(0);
    }
}

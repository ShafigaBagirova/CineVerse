using Application.Movies.Commands;
using FluentValidation;

namespace Application.Validations.Movie;

public sealed class UpdateReviewCommandValidator
    : AbstractValidator<UpdateReviewCommand>
{
    public UpdateReviewCommandValidator()
    {
        RuleFor(x => x.MovieId)
            .GreaterThan(0);

        RuleFor(x => x.Request.Content)
            .NotEmpty()
            .MaximumLength(1000);
    }
}
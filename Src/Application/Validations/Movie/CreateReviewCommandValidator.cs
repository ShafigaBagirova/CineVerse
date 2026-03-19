using Application.Movies.Commands;
using FluentValidation;

namespace Application.Validations.Movie;

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
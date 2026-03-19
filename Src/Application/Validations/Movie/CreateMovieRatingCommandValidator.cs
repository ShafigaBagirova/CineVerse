using Application.Movies.Commands;
using FluentValidation;

namespace Application.Validations.Movie;

public class CreateMovieRatingCommandValidator: AbstractValidator<CreateMovieRatingCommand>
{
    public CreateMovieRatingCommandValidator()
    {
        RuleFor(x => x.MovieId)
            .GreaterThan(0)
            .WithMessage("MovieId must be greater than 0.");

        RuleFor(x => x.Request.Rating)
            .GreaterThanOrEqualTo(1)
            .LessThanOrEqualTo(10)
            .WithMessage("Rating must be between 1 and 10.");
    }
}

using Application.Movies.Dtos;
using FluentValidation;

namespace Application.Validations.Movie;

public class CreateMovieRatingRequestValidator: AbstractValidator<CreateMovieRatingRequest>
{
    public CreateMovieRatingRequestValidator()
    {
        RuleFor(x => x.Rating)
            .GreaterThanOrEqualTo(1)
            .LessThanOrEqualTo(10)
            .WithMessage("Rating must be between 1 and 10.");
    }
}

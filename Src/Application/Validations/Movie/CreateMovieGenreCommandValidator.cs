using Application.Movies.Commands;
using FluentValidation;

namespace Application.Validations.Movie;

public class CreateMovieGenreCommandValidator : AbstractValidator<CreateMovieGenreCommand>
{
    public CreateMovieGenreCommandValidator()
    {

        RuleFor(x => x.MovieId)
             .GreaterThan(0).WithMessage("MovieId must be greater than 0.");

        RuleFor(x => x.Request.GenreId)
            .GreaterThan(0).WithMessage("GenreId must be greater than 0.");

        RuleFor(x => x.Request.Order)
            .GreaterThanOrEqualTo(0).WithMessage("Order must be 0 or greater.");
    }
}

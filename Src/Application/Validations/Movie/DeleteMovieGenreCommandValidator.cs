using Application.Movies.Commands;
using FluentValidation;

namespace Application.Validations.Movie;


public sealed class DeleteMovieGenreCommandValidator
    : AbstractValidator<DeleteMovieGenreCommand>
{
    public DeleteMovieGenreCommandValidator()
    {
        RuleFor(x => x.MovieId)
            .GreaterThan(0)
            .WithMessage("MovieId must be greater than 0.");

        RuleFor(x => x.GenreId)
            .GreaterThan(0)
            .WithMessage("GenreId must be greater than 0.");
    }
}
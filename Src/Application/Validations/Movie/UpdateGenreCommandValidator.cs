using Application.Movies.Commands;
using Application.Movies.Dtos;
using FluentValidation;

namespace Application.Validations.Movie;

public class UpdateGenreCommandValidator : AbstractValidator<UpdateGenreCommand>
{
    public UpdateGenreCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Genre name is required.")
            .MaximumLength(100).WithMessage("Genre name must not exceed 100 characters.");
    }
}

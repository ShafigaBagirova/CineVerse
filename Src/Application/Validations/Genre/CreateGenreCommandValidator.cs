using Application.Genres.Commands;
using Application.Movies.Dtos;
using FluentValidation;

namespace Application.Validations.Genre;

public class CreateGenreCommandValidator : AbstractValidator<CreateGenreCommand>
{
    public CreateGenreCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Genre name is required.")
            .MaximumLength(100).WithMessage("Genre name must not exceed 100 characters.");
    }
}
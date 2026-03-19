using Application.Movies.Commands;
using Application.Movies.Dtos;
using FluentValidation;

namespace Application.Validations.Movie;
public sealed class CreateMovieCommandValidator : AbstractValidator<CreateMovieCommand>
{
    public CreateMovieCommandValidator()
    {
        RuleFor(x => x.CreateMovieRequest.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200);

        RuleFor(x => x.CreateMovieRequest.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(4000);

        RuleFor(x => x.CreateMovieRequest.Country)
            .NotEmpty().WithMessage("Country is required.")
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.CreateMovieRequest.Country));

        RuleFor(x => x.CreateMovieRequest.AgeRating)
            .NotEmpty().WithMessage("Age rating is required.")
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.CreateMovieRequest.AgeRating));

        RuleFor(x => x.CreateMovieRequest.Tagline)
            .NotEmpty().WithMessage("Tagline is required.")
            .MaximumLength(300)
            .When(x => !string.IsNullOrWhiteSpace(x.CreateMovieRequest.Tagline));

        RuleFor(x => x.CreateMovieRequest.Director)
            .NotEmpty().WithMessage("Director is required.")
            .MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.CreateMovieRequest.Director));

        RuleFor(x => x.CreateMovieRequest.Language)
            .NotEmpty().WithMessage("Language is required.")
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.CreateMovieRequest.Language));

        RuleFor(x => x.CreateMovieRequest.DurationMinutes)
            .GreaterThan(0)
            .LessThanOrEqualTo(600)
            .WithMessage("Duration must be between 1 and 600 minutes.");


        RuleFor(x => x.CreateMovieRequest.TmdbId)
            .GreaterThan(0);

        RuleFor(x => x.CreateMovieRequest.ReleaseDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(5)));
    }
}
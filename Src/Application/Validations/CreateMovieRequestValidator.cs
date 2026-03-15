using Application.Movies.Dtos;
using FluentValidation;

namespace Application.Validations;
public sealed class CreateMovieRequestValidator : AbstractValidator<CreateMovieRequest>
{
    public CreateMovieRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(4000);

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.")
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Country));

        RuleFor(x => x.AgeRating)
            .NotEmpty().WithMessage("Age rating is required.")
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.AgeRating));

        RuleFor(x => x.Tagline)
            .NotEmpty().WithMessage("Tagline is required.")
            .MaximumLength(300)
            .When(x => !string.IsNullOrWhiteSpace(x.Tagline));

        RuleFor(x => x.Director)
            .NotEmpty().WithMessage("Director is required.")
            .MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.Director));

        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Language is required.")
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.Language));

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0)
            .LessThanOrEqualTo(600)
            .WithMessage("Duration must be between 1 and 600 minutes.");


        RuleFor(x => x.TmdbId)
            .GreaterThan(0);

        RuleFor(x => x.ReleaseDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(5)));
    }
}
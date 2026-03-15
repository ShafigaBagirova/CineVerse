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
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Country));

        RuleFor(x => x.AgeRating)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.AgeRating));

        RuleFor(x => x.Tagline)
            .MaximumLength(300)
            .When(x => !string.IsNullOrWhiteSpace(x.Tagline));

        RuleFor(x => x.Director)
            .MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.Director));

        RuleFor(x => x.Language)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.Language));

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0)
            .LessThanOrEqualTo(600)
            .WithMessage("Duration must be between 1 and 600 minutes.");

        RuleFor(x => x.ImdbRating)
            .InclusiveBetween(0, 10)
            .When(x => x.ImdbRating.HasValue);

        RuleFor(x => x.Slug)
            .NotEmpty()
            .MaximumLength(250)
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug must be lowercase and contain only letters, numbers and hyphens.");

        RuleFor(x => x.Status)
            .IsInEnum();

        RuleFor(x => x.TmdbId)
            .GreaterThan(0)
            .When(x => x.TmdbId.HasValue);

        RuleFor(x => x.ReleaseDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(5)))
            .When(x => x.ReleaseDate.HasValue);
    }
}
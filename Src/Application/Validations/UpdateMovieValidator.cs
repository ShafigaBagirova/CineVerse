using Application.Movies.Dtos;
using FluentValidation;

namespace Application.Validations;

public sealed class UpdateMovieRequestValidator : AbstractValidator<UpdateMovieRequest>
{
    public UpdateMovieRequestValidator()
    {

        RuleFor(x => x.Title)
            .MaximumLength(200);

        RuleFor(x => x.Description)
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

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid movie status.");

        RuleFor(x => x.ReleaseDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(5)))
            .When(x => x.ReleaseDate.HasValue);
    }
}

using Application.Movies.Commands;
using Application.Movies.Dtos;
using FluentValidation;

namespace Application.Validations.Movie;

public sealed class UpdateMovieCommandValidator : AbstractValidator<UpdateMovieCommand>
{
    public UpdateMovieCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty()
            .GreaterThan(0)
            .WithMessage("MovieId must be greater than 0.");

        RuleFor(x => x.UpdateMovieRequest.Title)
            .MaximumLength(200);

        RuleFor(x => x.UpdateMovieRequest.Description)
            .MaximumLength(4000);

        RuleFor(x => x.UpdateMovieRequest.Country)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.UpdateMovieRequest.Country));

        RuleFor(x => x.UpdateMovieRequest.AgeRating)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.UpdateMovieRequest.AgeRating));

        RuleFor(x => x.UpdateMovieRequest.Tagline)
            .MaximumLength(300)
            .When(x => !string.IsNullOrWhiteSpace(x.UpdateMovieRequest.Tagline));

        RuleFor(x => x.UpdateMovieRequest.Director)
            .MaximumLength(150)
            .When(x => !string.IsNullOrWhiteSpace(x.UpdateMovieRequest.Director));

        RuleFor(x => x.UpdateMovieRequest.Language)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.UpdateMovieRequest.Language));

        RuleFor(x => x.UpdateMovieRequest.DurationMinutes)
            .GreaterThan(0)
            .LessThanOrEqualTo(600)
            .WithMessage("Duration must be between 1 and 600 minutes.");

        RuleFor(x => x.UpdateMovieRequest.Status)
            .IsInEnum()
            .WithMessage("Invalid movie status.");

        RuleFor(x => x.UpdateMovieRequest.ReleaseDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(5)))
            .When(x => x.UpdateMovieRequest.ReleaseDate.HasValue);
    }
}

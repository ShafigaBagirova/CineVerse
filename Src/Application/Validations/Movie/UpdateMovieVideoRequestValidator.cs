using Application.MovieVideos.Dtos;
using FluentValidation;

namespace Application.Validations.Movie;

public sealed class UpdateMovieVideoValidator : AbstractValidator<UpdateMovieVideoRequest>
{
    public UpdateMovieVideoValidator()
    {
        RuleFor(x => x.MovieId)
            .GreaterThan(0)
            .When(x => x.MovieId.HasValue)
            .WithMessage("MovieId must be greater than 0.");

        RuleFor(x => x.VideoKey)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.VideoKey))
            .WithMessage("VideoKey cannot exceed 200 characters.");

        RuleFor(x => x.Site)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.Site))
            .WithMessage("Site cannot exceed 50 characters.");

        RuleFor(x => x.Type)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.Type))
            .WithMessage("Type cannot exceed 50 characters.")
            .Must(x => x == "Trailer" || x == "Teaser" || x == "Clip")
            .WithMessage("Type must be Trailer, Teaser, or Clip.") ;

        RuleFor(x => x.Name)
            .MaximumLength(300)
            .When(x => !string.IsNullOrWhiteSpace(x.Name))
            .WithMessage("Name cannot exceed 300 characters.");
    }
}
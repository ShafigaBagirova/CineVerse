using Application.MovieVideos.Dtos;
using FluentValidation;

namespace Application.Validations.Movie;


public sealed class CreateMovieVideoValidator : AbstractValidator<CreateMovieVideoRequest>
{
    public CreateMovieVideoValidator()
    {
        RuleFor(x => x.MovieId)
            .GreaterThan(0)
            .WithMessage("MovieId must be greater than 0.");

        RuleFor(x => x.VideoKey)
            .NotEmpty()
            .MaximumLength(200)
            .WithMessage("VideoKey is required and cannot exceed 200 characters.");

        RuleFor(x => x.Site)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Site is required and cannot exceed 50 characters.");

        RuleFor(x => x.Type)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Type is required and cannot exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(300)
            .WithMessage("Name is required and cannot exceed 300 characters.");
    }
}
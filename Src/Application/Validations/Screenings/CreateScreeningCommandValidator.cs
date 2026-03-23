using Application.Screenings.Commands;
using FluentValidation;

namespace Application.Validations.Screenings;

public sealed class CreateScreeningCommandValidator : AbstractValidator<CreateScreeningCommand>
{
    public CreateScreeningCommandValidator()
    {
        RuleFor(x => x.Request.MovieId)
            .GreaterThan(0)
            .WithMessage("Movie id must be greater than 0.");

        RuleFor(x => x.Request.HallId)
            .GreaterThan(0)
            .WithMessage("Hall id must be greater than 0.");

        RuleFor(x => x.Request.StartTime)
            .NotEmpty()
            .WithMessage("Start time is required.");

        RuleFor(x => x.Request.EndTime)
            .NotEmpty()
            .WithMessage("End time is required.");

        RuleFor(x => x.Request)
            .Must(x => x.EndTime > x.StartTime)
            .WithMessage("End time must be greater than start time.");

        RuleFor(x => x.Request.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0.");

        RuleFor(x => x.Request.Language)
            .NotEmpty()
            .WithMessage("Language is required.")
            .MaximumLength(20)
            .WithMessage("Language must not exceed 20 characters.");

        When(x => !string.IsNullOrWhiteSpace(x.Request.SubtitleLanguage), () =>
        {
            RuleFor(x => x.Request.SubtitleLanguage!)
                .MaximumLength(20)
                .WithMessage("Subtitle language must not exceed 20 characters.");
        });

        RuleFor(x => x.Request.Format)
            .IsInEnum()
            .WithMessage("Screening format is invalid.");
    }
}
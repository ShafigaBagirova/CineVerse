using Application.Screenings.Commands;
using FluentValidation;

namespace Application.Validations.Screenings;

public sealed class UpdateScreeningCommandValidator : AbstractValidator<UpdateScreeningCommand>
{
    public UpdateScreeningCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Screening id must be greater than 0.");

        When(x => x.Request.MovieId.HasValue, () =>
        {
            RuleFor(x => x.Request.MovieId!.Value)
                .GreaterThan(0)
                .WithMessage("Movie id must be greater than 0.");
        });

        When(x => x.Request.HallId.HasValue, () =>
        {
            RuleFor(x => x.Request.HallId!.Value)
                .GreaterThan(0)
                .WithMessage("Hall id must be greater than 0.");
        });

        When(x => x.Request.Price.HasValue, () =>
        {
            RuleFor(x => x.Request.Price!.Value)
                .GreaterThan(0)
                .WithMessage("Price must be greater than 0.");
        });

        When(x => x.Request.Language is not null, () =>
        {
            RuleFor(x => x.Request.Language!)
                .NotEmpty()
                .WithMessage("Language cannot be empty.")
                .MaximumLength(20)
                .WithMessage("Language must not exceed 20 characters.");
        });

        When(x => x.Request.SubtitleLanguage is not null, () =>
        {
            RuleFor(x => x.Request.SubtitleLanguage!)
                .MaximumLength(20)
                .WithMessage("Subtitle language must not exceed 20 characters.");
        });

        When(x => x.Request.Format.HasValue, () =>
        {
            RuleFor(x => x.Request.Format!.Value)
                .IsInEnum()
                .WithMessage("Screening format is invalid.");
        });

        When(x => x.Request.Status.HasValue, () =>
        {
            RuleFor(x => x.Request.Status!.Value)
                .IsInEnum()
                .WithMessage("Screening status is invalid.");
        });

        When(x => x.Request.StartTime.HasValue && x.Request.EndTime.HasValue, () =>
        {
            RuleFor(x => x.Request)
                .Must(r => r.StartTime < r.EndTime)
                .WithMessage("End time must be greater than start time.");
        });
    }
}
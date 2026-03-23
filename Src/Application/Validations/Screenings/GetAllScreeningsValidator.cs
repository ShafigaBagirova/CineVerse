using Application.Screenings.Queries;
using FluentValidation;

namespace Application.Validations.Screenings;

public sealed class GetAllScreeningsQueryValidator : AbstractValidator<GetAllScreeningsQuery>
{
    public GetAllScreeningsQueryValidator()
    {
        RuleFor(x => x.Request.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.Request.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("Page size must be between 1 and 100.");

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

        When(x => x.Request.Status.HasValue, () =>
        {
            RuleFor(x => x.Request.Status!.Value)
                .IsInEnum()
                .WithMessage("Screening status is invalid.");
        });

        When(x => x.Request.Format.HasValue, () =>
        {
            RuleFor(x => x.Request.Format!.Value)
                .IsInEnum()
                .WithMessage("Screening format is invalid.");
        });

        When(x => x.Request.DateFrom.HasValue && x.Request.DateTo.HasValue, () =>
        {
            RuleFor(x => x.Request)
                .Must(r => r.DateFrom <= r.DateTo)
                .WithMessage("DateFrom must be less than or equal to DateTo.");
        });
    }
}

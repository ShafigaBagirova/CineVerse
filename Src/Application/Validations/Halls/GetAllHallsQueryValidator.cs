using Application.Halls.Queries;
using FluentValidation;

namespace Application.Validations.Halls;

public sealed class GetAllHallsQueryValidator : AbstractValidator<GetAllHallsQuery>
{
    public GetAllHallsQueryValidator()
    {
        RuleFor(x => x.Request.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.Request.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("Page size must be between 1 and 100.");

        When(x => x.Request.CinemaId.HasValue, () =>
        {
            RuleFor(x => x.Request.CinemaId!.Value)
                .GreaterThan(0)
                .WithMessage("Cinema id must be greater than 0.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Request.Search), () =>
        {
            RuleFor(x => x.Request.Search!)
                .MaximumLength(100)
                .WithMessage("Search must not exceed 100 characters.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Request.SortBy), () =>
        {
            RuleFor(x => x.Request.SortBy!)
                .Must(x => new[] { "name", "capacity", "createdat" }
                    .Contains(x.ToLower()))
                .WithMessage("SortBy must be one of: name, capacity, createdAt.");
        });
    }
}
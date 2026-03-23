using Application.Cinemas.Queries;
using FluentValidation;

namespace Application.Validations.Cinema;

public sealed class GetAllCinemasQueryValidator : AbstractValidator<GetAllCinemasQuery>
{
    public GetAllCinemasQueryValidator()
    {
        RuleFor(x => x.Request.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.Request.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("Page size must be between 1 and 100.");

        When(x => !string.IsNullOrWhiteSpace(x.Request.Country), () =>
        {
            RuleFor(x => x.Request.Country!)
                .MaximumLength(50)
                .WithMessage("Country must not exceed 50 characters.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Request.City), () =>
        {
            RuleFor(x => x.Request.City!)
                .MaximumLength(50)
                .WithMessage("City must not exceed 50 characters.");
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
                .Must(x => new[] { "name", "city", "country", "createdat" }
                    .Contains(x.ToLower()))
                .WithMessage("SortBy must be one of: name, city, country, createdAt.");
        });
    }
}
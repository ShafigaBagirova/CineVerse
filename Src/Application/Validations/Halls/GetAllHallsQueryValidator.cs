using Application.Halls.Queries;
using FluentValidation;

namespace Application.Validations.Halls;

public class GetAllHallsQueryValidator : AbstractValidator<GetAllHallsQuery>
{
    public GetAllHallsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(50)
            .WithMessage("Page size must be between 1 and 50.");

        RuleFor(x => x.CinemaId)
            .GreaterThan(0)
            .When(x => x.CinemaId.HasValue)
            .WithMessage("Cinema id must be greater than 0.");

        RuleFor(x => x.Search)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Search))
            .WithMessage("Search must not exceed 100 characters.");

        RuleFor(x => x.SortBy)
            .Must(x => string.IsNullOrWhiteSpace(x) ||
                       x.Trim().ToLower() is "name" or "capacity" or "createdat")
            .WithMessage("SortBy must be one of: name, capacity, createdAt.");
    }
}
using Application.Cinemas.Queries;
using FluentValidation;

namespace Application.Validations.Cinema;

public class GetAllCinemasQueryValidator : AbstractValidator<GetAllCinemasQuery>
{
    public GetAllCinemasQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0.")
            .LessThanOrEqualTo(50).WithMessage("PageSize must be maximum 50.");

        RuleFor(x => x.Country)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Country));

        RuleFor(x => x.City)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.City));

        RuleFor(x => x.Search)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Search));

        RuleFor(x => x.SortBy)
            .Must(x => string.IsNullOrWhiteSpace(x) ||
                       x.ToLower() is "name" or "city" or "country")
            .WithMessage("SortBy can be only city or country.");
    }
}
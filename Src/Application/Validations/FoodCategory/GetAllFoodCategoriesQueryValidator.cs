using Application.FoodCategories.Queries;
using FluentValidation;

namespace Application.Validations.FoodCategory;

public class GetAllFoodCategoriesQueryValidator : AbstractValidator<GetAllFoodCategoriesQuery>
{
    public GetAllFoodCategoriesQueryValidator()
    {
        RuleFor(x => x.Request.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.Request.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(50)
            .WithMessage("Page size must be between 1 and 50.");

        RuleFor(x => x.Request.CinemaId)
            .GreaterThan(0)
            .When(x => x.Request.CinemaId.HasValue)
            .WithMessage("Cinema id must be greater than 0.");

        RuleFor(x => x.Request.Search)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Request.Search))
            .WithMessage("Search must not exceed 100 characters.");
    }
}
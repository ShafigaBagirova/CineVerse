using Application.FoodCategories.Queries;
using FluentValidation;

namespace Application.Validations.FoodCategory;

public class GetFoodCategoriesWithItemsQueryValidator
    : AbstractValidator<GetFoodCategoriesWithItemsQuery>
{
    public GetFoodCategoriesWithItemsQueryValidator()
    {
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
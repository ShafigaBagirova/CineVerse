using Application.FoodCategories.Queries;
using FluentValidation;

namespace Application.Validations.FoodCategory;

public class GetFoodCategoryByIdQueryValidator : AbstractValidator<GetFoodCategoryByIdQuery>
{
    public GetFoodCategoryByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Food category id must be greater than 0.");
    }
}
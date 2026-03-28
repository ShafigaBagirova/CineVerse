using Application.FoodItems.Queries;
using FluentValidation;

namespace Application.Validations.FoodItems;

public class GetFoodItemByIdQueryValidator : AbstractValidator<GetFoodItemByIdQuery>
{
    public GetFoodItemByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Food item id must be greater than 0.");
    }
}
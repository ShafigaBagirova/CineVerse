using Application.FoodCategories.Commands;
using FluentValidation;

namespace Application.Validations.Food;

public sealed class DeleteFoodCategoryCommandValidator
    : AbstractValidator<DeleteFoodCategoryCommand>
{
    public DeleteFoodCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}
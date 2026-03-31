using Application.FoodCategories.Commands;
using FluentValidation;

namespace Application.Validations.Food;

public sealed class UpdateFoodCategoryCommandValidator
    : AbstractValidator<UpdateFoodCategoryCommand>
{
    public UpdateFoodCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Request.Name)
            .MaximumLength(100);

        RuleFor(x => x.Request.Description)
            .MaximumLength(500);
    }
}
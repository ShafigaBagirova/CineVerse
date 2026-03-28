using Application.FoodCategories.Commands;
using FluentValidation;

namespace Application.Validations.Food;
public sealed class CreateFoodCategoryCommandValidator
    : AbstractValidator<CreateFoodCategoryCommand>
{
    public CreateFoodCategoryCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Request.Description)
            .MaximumLength(500);

        RuleFor(x => x.Request.DisplayOrder)
            .GreaterThanOrEqualTo(0);
    }
}

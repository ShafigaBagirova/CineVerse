using Application.FoodItems.Commands;
using FluentValidation;

namespace Application.Validations.FoodItems;

public sealed class DeleteFoodItemCommandValidator
    : AbstractValidator<DeleteFoodItemCommand>
{
    public DeleteFoodItemCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}
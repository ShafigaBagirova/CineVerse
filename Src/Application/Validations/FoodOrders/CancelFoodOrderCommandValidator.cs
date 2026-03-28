using Application.FoodOrders.Commands;
using FluentValidation;

namespace Application.Validations.FoodOrders;

public sealed class CancelFoodOrderCommandValidator
    : AbstractValidator<CancelFoodOrderCommand>
{
    public CancelFoodOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);
    }
}
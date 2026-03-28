using Application.FoodOrders.Commands;
using FluentValidation;

namespace Application.Validations.FoodOrders;

public sealed class CreateFoodOrderDraftCommandValidator
    : AbstractValidator<CreateFoodOrderDraftCommand>
{
    public CreateFoodOrderDraftCommandValidator()
    {
        RuleFor(x => x.Request.SeatHoldId)
            .NotEmpty();

        RuleFor(x => x.Request.Items)
            .NotEmpty();

        RuleForEach(x => x.Request.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.FoodItemId)
                .GreaterThan(0);

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0);
        });

        RuleFor(x => x.Request.Note)
            .MaximumLength(1000);
    }
}
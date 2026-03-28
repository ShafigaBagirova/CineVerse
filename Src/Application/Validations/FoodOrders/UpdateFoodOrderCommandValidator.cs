using Application.FoodOrders.Commands;
using FluentValidation;

namespace Application.Validations.FoodOrders;

public sealed class UpdateFoodOrderDraftCommandValidator
    : AbstractValidator<UpdateFoodOrderDraftCommand>
{
    public UpdateFoodOrderDraftCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

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
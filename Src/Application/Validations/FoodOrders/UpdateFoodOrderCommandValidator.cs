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
            .NotNull()
            .WithMessage("Items are required.")
            .Must(x => x.Count > 0)
            .WithMessage("At least one food item is required.");

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
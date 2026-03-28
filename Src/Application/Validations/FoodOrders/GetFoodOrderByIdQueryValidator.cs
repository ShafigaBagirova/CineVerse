using Application.FoodOrders.Queries;
using FluentValidation;

namespace Application.Validations.FoodOrders;

public class GetFoodOrderByIdQueryValidator : AbstractValidator<GetFoodOrderByIdQuery>
{
    public GetFoodOrderByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Food order id must be greater than 0.");
    }
}
using Application.FoodOrders.Queries;
using FluentValidation;

namespace Application.Validations.FoodOrders;

public class GetFoodOrdersByDayQueryValidator : AbstractValidator<GetFoodOrdersByDayQuery>
{
    public GetFoodOrdersByDayQueryValidator()
    {
        RuleFor(x => x.Request.CinemaId)
            .GreaterThan(0)
            .When(x => x.Request.CinemaId.HasValue)
            .WithMessage("Cinema id must be greater than 0.");

        RuleFor(x => x.Request.ScreeningId)
            .GreaterThan(0)
            .When(x => x.Request.ScreeningId.HasValue)
            .WithMessage("Screening id must be greater than 0.");

        RuleFor(x => x.Request)
            .Must(x => !x.From.HasValue || !x.To.HasValue || x.From <= x.To)
            .WithMessage("From date cannot be greater than To date.");
    }
}
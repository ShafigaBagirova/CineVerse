using Application.FoodOrders.Queries;
using FluentValidation;

namespace Application.Validations.FoodOrders;

public class GetFoodOrderSummaryQueryValidator : AbstractValidator<GetFoodOrderSummaryQuery>
{
    public GetFoodOrderSummaryQueryValidator()
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
            .Must(x => !x.CreatedFrom.HasValue || !x.CreatedTo.HasValue || x.CreatedFrom <= x.CreatedTo)
            .WithMessage("CreatedFrom cannot be greater than CreatedTo.");
    }
}
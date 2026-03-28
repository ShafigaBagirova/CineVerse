using Application.FoodOrders.Queries;
using FluentValidation;

namespace Application.Validations.FoodOrders;

public class GetMyFoodOrdersQueryValidator : AbstractValidator<GetMyFoodOrdersQuery>
{
    public GetMyFoodOrdersQueryValidator()
    {
        RuleFor(x => x.Request.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.Request.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(50)
            .WithMessage("Page size must be between 1 and 50.");

        RuleFor(x => x.Request.SeatHoldId)
            .GreaterThan(0)
            .When(x => x.Request.SeatHoldId.HasValue)
            .WithMessage("SeatHold id must be greater than 0.");

        RuleFor(x => x.Request.ScreeningId)
            .GreaterThan(0)
            .When(x => x.Request.ScreeningId.HasValue)
            .WithMessage("Screening id must be greater than 0.");

        RuleFor(x => x.Request.SeatId)
            .GreaterThan(0)
            .When(x => x.Request.SeatId.HasValue)
            .WithMessage("Seat id must be greater than 0.");

        RuleFor(x => x.Request.CinemaId)
            .GreaterThan(0)
            .When(x => x.Request.CinemaId.HasValue)
            .WithMessage("Cinema id must be greater than 0.");

        RuleFor(x => x.Request.Status)
           .IsInEnum()
           .When(x => x.Request.Status.HasValue)
           .WithMessage("Invalid food order status.");
        RuleFor(x => x.Request.DeliveryType)
            .IsInEnum()
            .When(x => x.Request.DeliveryType.HasValue)
            .WithMessage("Invalid delivery type");

        RuleFor(x => x.Request)
            .Must(x => !x.CreatedFrom.HasValue || !x.CreatedTo.HasValue || x.CreatedFrom <= x.CreatedTo)
            .WithMessage("CreatedFrom cannot be greater than CreatedTo.");
    }
}
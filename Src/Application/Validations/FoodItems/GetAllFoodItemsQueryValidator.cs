using Application.FoodItems.Queries;
using FluentValidation;

namespace Application.Validations.FoodItems;

public class GetAllFoodItemsQueryValidator : AbstractValidator<GetAllFoodItemsQuery>
{
    public GetAllFoodItemsQueryValidator()
    {
        RuleFor(x => x.Request.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.Request.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(50)
            .WithMessage("Page size must be between 1 and 50.");

        RuleFor(x => x.Request.CinemaId)
            .GreaterThan(0)
            .When(x => x.Request.CinemaId.HasValue)
            .WithMessage("Cinema id must be greater than 0.");

        RuleFor(x => x.Request.FoodCategoryId)
            .GreaterThan(0)
            .When(x => x.Request.FoodCategoryId.HasValue)
            .WithMessage("Food category id must be greater than 0.");

        RuleFor(x => x.Request.MinPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Request.MinPrice.HasValue)
            .WithMessage("Min price must be greater than or equal to 0.");

        RuleFor(x => x.Request.MaxPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Request.MaxPrice.HasValue)
            .WithMessage("Max price must be greater than or equal to 0.");

        RuleFor(x => x.Request)
            .Must(x => !x.MinPrice.HasValue || !x.MaxPrice.HasValue || x.MinPrice <= x.MaxPrice)
            .WithMessage("Min price cannot be greater than max price.");

        RuleFor(x => x.Request.Search)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Request.Search))
            .WithMessage("Search must not exceed 100 characters.");
    }
}
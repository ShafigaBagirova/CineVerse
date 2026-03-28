using Application.FoodItems.Commands;
using FluentValidation;

namespace Application.Validations.FoodItems;

public sealed class CreateFoodItemCommandValidator
    : AbstractValidator<CreateFoodItemCommand>
{
    public CreateFoodItemCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(x => x.Request.Description)
            .MaximumLength(1000);

        RuleFor(x => x.Request.Price)
            .GreaterThan(0);


        RuleFor(x => x.Request.Image)
            .Must(file => file == null ||
                          file.ContentType == "image/jpeg" ||
                          file.ContentType == "image/png" ||
                          file.ContentType == "image/webp")
            .WithMessage("Only JPEG, PNG, or WEBP images are allowed.");

        RuleFor(x => x.Request.FoodCategoryId)
            .GreaterThan(0);
    }
}
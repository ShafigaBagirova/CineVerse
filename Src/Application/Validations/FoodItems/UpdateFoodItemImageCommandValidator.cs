using Application.FoodItems.Commands;
using FluentValidation;

namespace Application.Validations.FoodItems;

public sealed class UpdateFoodItemImageRequestValidator : AbstractValidator<UpdateFoodItemImageCommand>
{
    private static readonly string[] AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    public UpdateFoodItemImageRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0.");

        RuleFor(x => x.Request.Image)
            .NotNull().WithMessage("Image is required.");

        When(x => x.Request.Image is not null, () =>
        {
            RuleFor(x => x.Request.Image!.Length)
                .GreaterThan(0).WithMessage("Image cannot be empty.");

            RuleFor(x => x.Request.Image!.ContentType)
                .Must(type => AllowedContentTypes.Contains(type))
                .WithMessage("Only JPEG, PNG, and WEBP images are allowed.");

            RuleFor(x => x.Request.Image!.Length)
                .LessThanOrEqualTo(5 * 1024 * 1024)
                .WithMessage("Image size cannot exceed 5 MB.");
        });
    }
}
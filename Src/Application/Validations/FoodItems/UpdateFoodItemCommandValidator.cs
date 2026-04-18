using Application.FoodItems.Commands;
using Application.FoodItems.Dtos;
using FluentValidation;

namespace Application.Validations.FoodItems;

 public sealed class UpdateFoodItemRequestValidator : AbstractValidator<UpdateFoodItemRequest>
    {
        public UpdateFoodItemRequestValidator()
        {
            RuleFor(x => x)
                .Must(HaveAtLeastOneField)
                .WithMessage("At least one field must be provided for update.");

            When(x => x.Name is not null, () =>
            {
                RuleFor(x => x.Name)
                    .Must(x => !string.IsNullOrWhiteSpace(x))
                    .WithMessage("Name cannot be empty.")
                    .MaximumLength(100);
            });

            When(x => x.Description is not null, () =>
            {
                RuleFor(x => x.Description)
                    .Must(x => !string.IsNullOrWhiteSpace(x))
                    .WithMessage("Description cannot be empty.")
                    .MaximumLength(500);
            });

            When(x => x.Price.HasValue, () =>
            {
                RuleFor(x => x.Price!.Value)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Price must be greater than or equal to 0.");
            });



            When(x => x.FoodCategoryId.HasValue, () =>
            {
                RuleFor(x => x.FoodCategoryId!.Value)
                    .GreaterThan(0)
                    .WithMessage("FoodCategoryId must be greater than 0.");
            });
        }

        private static bool HaveAtLeastOneField(UpdateFoodItemRequest request)
        {
            return request.Name is not null
                   || request.Description is not null
                   || request.Price.HasValue
                   || request.IsAvailable.HasValue
                   || request.IsActive.HasValue
                   || request.FoodCategoryId.HasValue;
        }
  }



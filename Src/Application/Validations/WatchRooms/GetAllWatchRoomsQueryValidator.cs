using Application.WatchRooms.Queries;
using FluentValidation;

namespace Application.Validations.WatchRooms;

public sealed class GetAllWatchRoomsQueryValidator : AbstractValidator<GetAllWatchRoomsQuery>
{
    public GetAllWatchRoomsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("Page size must be between 1 and 100.");

        When(x => !string.IsNullOrWhiteSpace(x.Search), () =>
        {
            RuleFor(x => x.Search!)
                .MaximumLength(100)
                .WithMessage("Search text cannot exceed 100 characters.");
        });
    }
}

using Application.WatchListItems.Queries;
using FluentValidation;

namespace Application.Validations.WatchList;

public sealed class GetMyWatchlistQueryValidator
    : AbstractValidator<GetMyWatchlistQuery>
{
    public GetMyWatchlistQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(50)
            .WithMessage("PageSize must be between 1 and 50.");
    }
}
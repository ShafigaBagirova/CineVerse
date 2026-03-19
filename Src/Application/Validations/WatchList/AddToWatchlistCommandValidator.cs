using Application.WatchListItems.Commands;
using FluentValidation;

namespace Application.Validations.WatchList;

public sealed class AddToWatchlistCommandValidator
    : AbstractValidator<AddToWatchlistCommand>
{
    public AddToWatchlistCommandValidator()
    {
        RuleFor(x => x.MovieId)
            .GreaterThan(0)
            .WithMessage("MovieId must be greater than 0.");
    }
}
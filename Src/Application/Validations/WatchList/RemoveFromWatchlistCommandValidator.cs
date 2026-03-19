using Application.WatchListItems.Commands;
using FluentValidation;

namespace Application.Validations.WatchList;

public sealed class RemoveFromWatchlistCommandValidator
    : AbstractValidator<RemoveFromWatchlistCommand>
{
    public RemoveFromWatchlistCommandValidator()
    {
        RuleFor(x => x.MovieId)
            .GreaterThan(0)
            .WithMessage("MovieId must be greater than 0.");
    }
}
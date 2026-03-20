using Application.Watched.Commands;
using FluentValidation;

namespace Application.Validations.Watched;
public sealed class RemoveFromWatchedCommandValidator
    : AbstractValidator<RemoveFromWatchedCommand>
{
    public RemoveFromWatchedCommandValidator()
    {
        RuleFor(x => x.MovieId)
            .GreaterThan(0);
    }
}
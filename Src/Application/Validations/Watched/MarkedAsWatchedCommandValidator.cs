using Application.Watched.Commands;
using FluentValidation;

namespace Application.Validations.Watched;
public sealed class MarkAsWatchedCommandValidator
    : AbstractValidator<MarkAsWatchedCommand>
{
    public MarkAsWatchedCommandValidator()
    {
        RuleFor(x => x.MovieId)
            .GreaterThan(0);
    }
}
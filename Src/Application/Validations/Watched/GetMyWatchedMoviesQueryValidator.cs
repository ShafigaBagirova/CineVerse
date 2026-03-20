using Application.Watched.Queries;
using FluentValidation;

namespace Application.Validations.Watched;

public sealed class GetMyWatchedMoviesQueryValidator
    : AbstractValidator<GetMyWatchedMoviesQuery>
{
    public GetMyWatchedMoviesQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(50);
    }
}
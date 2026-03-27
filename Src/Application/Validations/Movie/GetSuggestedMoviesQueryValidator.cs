using Application.Movies.Queries;
using FluentValidation;

namespace Application.Validations.Movie;

public sealed class GetSuggestedMoviesQueryValidator : AbstractValidator<GetSuggestedMoviesQuery>
{
    public GetSuggestedMoviesQueryValidator()
    {
        RuleFor(x => x.Request.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.Request.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");
    }
}
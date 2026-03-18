using Application.Movies.Queries;
using FluentValidation;

namespace Application.Validations.Movie;

public sealed class GetMoviesByTmdbRatingRangeQueryValidator
    : AbstractValidator<GetMoviesByTmdbRatingRangeQuery>
{
    public GetMoviesByTmdbRatingRangeQueryValidator()
    {
        RuleFor(x => x.MinRating)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.MaxRating)
            .LessThanOrEqualTo(10);

        RuleFor(x => x)
            .Must(x => x.MinRating <= x.MaxRating)
            .WithMessage("MinRating cannot be greater than MaxRating.");
    }
}
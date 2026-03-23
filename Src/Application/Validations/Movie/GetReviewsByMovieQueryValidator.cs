using Application.Reviews.Queries;
using FluentValidation;

namespace Application.Validations.Movie;

public sealed class GetReviewsByMovieQueryValidator
    : AbstractValidator<GetReviewsByMovieQuery>
{
    public GetReviewsByMovieQueryValidator()
    {
        RuleFor(x => x.MovieId)
            .GreaterThan(0)
            .WithMessage("MovieId must be greater than 0.");
    }
}
using Application.Movies.Queries;
using FluentValidation;

namespace Application.Validations.Movie;

public sealed class GetMyReviewQueryValidator
    : AbstractValidator<GetMyReviewQuery>
{
    public GetMyReviewQueryValidator()
    {
        RuleFor(x => x.MovieId)
            .GreaterThan(0)
            .WithMessage("MovieId must be greater than 0.");
    }
}
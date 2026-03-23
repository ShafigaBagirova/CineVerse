using Application.Reviews.Queries;
using FluentValidation;

namespace Application.Validations.Review;

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
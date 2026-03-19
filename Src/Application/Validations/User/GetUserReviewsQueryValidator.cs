using Application.Auth.User.Queries;
using FluentValidation;

namespace Application.Validations.User;

public sealed class GetUserReviewsQueryValidator
    : AbstractValidator<GetUserReviewsQuery>
{
    public GetUserReviewsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Page)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(50);
    }
}
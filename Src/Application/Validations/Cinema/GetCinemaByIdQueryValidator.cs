using Application.Cinemas.Queries;
using FluentValidation;

namespace Application.Validations.Cinema;

public class GetCinemaByIdQueryValidator : AbstractValidator<GetCinemaByIdQuery>
{
    public GetCinemaByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Cinema Id 0-dan böyük olmalıdır.");
    }
}
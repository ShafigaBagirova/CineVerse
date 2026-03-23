using Application.Cinemas.Queries;
using FluentValidation;

namespace Application.Validations.Cinema;

public class GetCinemaByIdQueryValidator : AbstractValidator<GetCinemaByIdQuery>
{
    public GetCinemaByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Cinema Id must be greater than 0.");
    }
}
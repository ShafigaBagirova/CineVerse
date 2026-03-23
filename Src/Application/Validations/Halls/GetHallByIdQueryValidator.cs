using Application.Halls.Queries;
using FluentValidation;

namespace Application.Validations.Halls;

public class GetHallByIdQueryValidator : AbstractValidator<GetHallByIdQuery>
{
    public GetHallByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Hall id must be greater than 0.");
    }
}
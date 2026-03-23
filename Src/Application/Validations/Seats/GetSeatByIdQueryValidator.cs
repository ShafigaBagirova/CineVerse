using Application.Seats.Queries;
using FluentValidation;

namespace Application.Validations.Seats;

public class GetSeatByIdQueryValidator:AbstractValidator<GetSeatByIdQuery>
{
    public GetSeatByIdQueryValidator()
    {

        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Seat id must be greater than 0.");
    }
}

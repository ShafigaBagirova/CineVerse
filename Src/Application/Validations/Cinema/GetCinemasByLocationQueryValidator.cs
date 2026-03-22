using Application.Cinemas.Queries;
using FluentValidation;

namespace Application.Validations.Cinema;

public class GetCinemasByLocationQueryValidator : AbstractValidator<GetCinemasByLocationQuery>
{
    public GetCinemasByLocationQueryValidator()
    {
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Country) || !string.IsNullOrWhiteSpace(x.City))
            .WithMessage("At least one country or city must be send.");

        RuleFor(x => x.Country)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Country));

        RuleFor(x => x.City)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.City));
    }
}

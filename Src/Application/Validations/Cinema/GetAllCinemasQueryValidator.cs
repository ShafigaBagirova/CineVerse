using Application.Cinemas.Queries;
using FluentValidation;

namespace Application.Validations.Cinema;

public class GetAllCinemasQueryValidator : AbstractValidator<GetAllCinemasQuery>
{
    public GetAllCinemasQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("PageNumber 0-dan böyük olmalıdır.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize 0-dan böyük olmalıdır.")
            .LessThanOrEqualTo(50).WithMessage("PageSize maksimum 50 ola bilər.");
    }
}

using Application.Halls.Commands;
using FluentValidation;

namespace Application.Validations.Halls;

public class CreateHallCommandValidator : AbstractValidator<CreateHallCommand>
{
    public CreateHallCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Hall name boş ola bilməz.")
            .MaximumLength(100).WithMessage("Hall name maksimum 100 simvol ola bilər.");

        RuleFor(x => x.Request.CinemaId)
            .GreaterThan(0).WithMessage("CinemaId 0-dan böyük olmalıdır.");

        RuleFor(x => x.Request.Capacity)
            .GreaterThan(0).WithMessage("Capacity 0-dan böyük olmalıdır.");
    }
}

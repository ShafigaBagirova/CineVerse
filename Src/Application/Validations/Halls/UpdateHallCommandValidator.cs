using Application.Halls.Commands;
using FluentValidation;

namespace Application.Validations.Halls;

public class UpdateHallCommandValidator : AbstractValidator<UpdateHallCommand>
{
    public UpdateHallCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Hall id must be greater than 0.");

        RuleFor(x => x.Request.Name)
            .MaximumLength(100)
            .WithMessage("Hall name must not exceed 100 characters.");

        RuleFor(x => x.Request.CinemaId)
            .GreaterThan(0)
            .WithMessage("Cinema id must be greater than 0.");

        RuleFor(x => x.Request.Capacity)
            .GreaterThan(0)
            .WithMessage("Capacity must be greater than 0.");
    }
}
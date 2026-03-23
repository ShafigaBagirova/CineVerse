using Application.Seats.Commands;
using Application.Seats.Dtos;
using FluentValidation;

namespace Application.Validations.Seats;

public sealed class CreateSeatCommandValidator : AbstractValidator<CreateSeatCommand>
{
    public CreateSeatCommandValidator()
    {
        RuleFor(x => x.Request.HallId)
            .GreaterThan(0)
            .WithMessage("HallId must be greater than 0.");

        RuleFor(x => x.Request.Row)
            .NotEmpty()
            .WithMessage("Row is required.")
            .MaximumLength(10)
            .WithMessage("Row must not exceed 10 characters.");

        RuleFor(x => x.Request.Number)
            .GreaterThan(0)
            .WithMessage("Seat number must be greater than 0.");

        RuleFor(x => x.Request.Type)
            .IsInEnum()
            .WithMessage("Seat type is invalid.");
    }
}

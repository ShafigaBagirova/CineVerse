using Application.Seats.Commands;
using FluentValidation;

namespace Application.Validations.Seats;

public sealed class UpdateSeatCommandValidator : AbstractValidator<UpdateSeatCommand>
{
    public UpdateSeatCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Seat id must be greater than 0.");

        When(x => x.Request.HallId.HasValue, () =>
        {
            RuleFor(x => x.Request.HallId!.Value)
                .GreaterThan(0)
                .WithMessage("Hall id must be greater than 0.");
        });

        When(x => x.Request.Row is not null, () =>
        {
            RuleFor(x => x.Request.Row)
                .NotEmpty()
                .WithMessage("Row cannot be empty.")
                .MaximumLength(10)
                .WithMessage("Row must not exceed 10 characters.");
        });

        When(x => x.Request.Number.HasValue, () =>
        {
            RuleFor(x => x.Request.Number!.Value)
                .GreaterThan(0)
                .WithMessage("Seat number must be greater than 0.");
        });

        When(x => x.Request.Type.HasValue, () =>
        {
            RuleFor(x => x.Request.Type!.Value)
                .IsInEnum()
                .WithMessage("Seat type is invalid.");
        });
    }
}
using Application.Cinemas.Commands;
using FluentValidation;

namespace Application.Validations.Cinema;

public class UpdateCinemaCommandValidator : AbstractValidator<UpdateCinemaCommand>
{
    public UpdateCinemaCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Cinema Id 0-dan böyük olmalıdır.");

        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Name boş ola bilməz.")
            .MaximumLength(200);

        RuleFor(x => x.Request.Description)
            .MaximumLength(1000);

        RuleFor(x => x.Request.Address)
            .NotEmpty().WithMessage("Address boş ola bilməz.")
            .MaximumLength(300);

        RuleFor(x => x.Request.Phone)
            .MaximumLength(30);

        RuleFor(x => x.Request.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Request.Email))
            .WithMessage("Email düzgün formatda deyil.");
    }
}
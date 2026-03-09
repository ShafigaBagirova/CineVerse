using Application.Auth.UserName.Dtos;
using FluentValidation;

namespace Application.Validations;

public sealed class UpdateUserNameRequestValidator : AbstractValidator<UpdateUserNameRequest>
{
    public UpdateUserNameRequestValidator()
    {

        RuleFor(x => x.NewUserName)
            .NotEmpty().WithMessage("New username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters.")
            .MaximumLength(50).WithMessage("Username must be at most 50 characters.")
            .Matches("^[a-zA-Z0-9._]+$").WithMessage("Username can contain only letters, digits, dot and underscore.");
    }
}

using Application.Auth.Register.Dtos;
using Application.Common.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace Application.Validations.Auth;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{

    private static readonly HashSet<string> CommonPasswords = new()
   {
    "password",
    "12345678",
    "Baku123",
    "11111111"
    };

    public RegisterRequestValidator(IUserUniquenessChecker checker)
    {
        RuleFor(x => x.UserName)
       .NotEmpty().WithMessage("Username is required.")
       .MinimumLength(3).WithMessage("Username must be at least 3 characters.")
       .MaximumLength(50).WithMessage("Username cannot exceed 50 characters.")
       .Matches(@"^[a-zA-Z0-9._]+$").WithMessage("Username can only contain letters, numbers, '.' and '_'.")
       .MustAsync(async (_, userName, ct) => !await checker.IsUserNameTakenAsync(userName, ct))
       .WithMessage("This username is already taken.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MustAsync(async (_, email, ct) => !await checker.IsEmailTakenAsync(email, ct))
            .WithMessage("This email is already registered.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .MaximumLength(100).WithMessage("Password cannot exceed 100 characters.")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"\d").WithMessage("Password must contain at least one number.")
            .Matches(@"[\W_]").WithMessage("Password must contain at least one special character.")
            .Must(p => !p.Contains(" ")).WithMessage("Password cannot contain spaces.")
            .Must(p => !CommonPasswords.Contains(p.ToLowerInvariant()))
            .WithMessage("Password is too common. Choose a stronger password.");

        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(200)
            .Must(name => name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).Length >= 2);

        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .LessThan(DateTime.Today);

        RuleFor(x => x.Gender)
            .IsInEnum();
    }
}



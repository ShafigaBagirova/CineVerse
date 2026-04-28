using CineVerse.Application.Users.Dtos.AuthDto;
using Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace Application.Validations.AuthValidation;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    private readonly UserManager<User> _userManager;

    public RegisterRequestValidator(UserManager<User> userManager)
    {
        _userManager = userManager;

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters long.")
            .MaximumLength(50).WithMessage("Username cannot exceed 50 characters.")
            .Matches("^[a-zA-Z0-9._]+$")
            .WithMessage("Username can only contain letters, numbers, '.' and '_'.")
            .MustAsync(async (model, userName, ct) =>
                await _userManager.FindByNameAsync(userName) == null)
            .WithMessage("This username is already taken.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters.")
            .MustAsync(async (model, email, ct) =>
                await _userManager.FindByEmailAsync(email) == null)
            .WithMessage("This email is already registered.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(100).WithMessage("Password cannot exceed 100 characters.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(200).WithMessage("Full name cannot exceed 200 characters.")
            .Must(name => name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).Length >= 2)
            .WithMessage("Full name must include at least first name and last name.");
       
        RuleFor(x => x.DateOfBirth)
       .NotEmpty().WithMessage("Date of birth is required.")
       .LessThan(DateTime.Today).WithMessage("Date of birth cannot be in the future.");

        RuleFor(x => x.Gender)
            .IsInEnum().WithMessage("Invalid gender value.");
    }
}



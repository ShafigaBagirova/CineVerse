using Application.Auth.User.Dtos;
using Application.Auth.User.Queries;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Validations;
public sealed class UserProfileValidator : AbstractValidator<UserProfileDto>
{
    public UserProfileValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}

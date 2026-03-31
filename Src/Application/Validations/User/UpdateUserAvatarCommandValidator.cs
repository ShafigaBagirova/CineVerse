using Application.Auth.User.Commands;
using FluentValidation;

namespace Application.Validations.User;

public sealed class UploadUserAvatarCommandValidator
    : AbstractValidator<UploadUserAvatarCommand>
{
    public UploadUserAvatarCommandValidator()
    {
        RuleFor(x => x.Request.Avatar)
            .NotNull().WithMessage("Avatar file is required.");

        RuleFor(x => x.Request.Avatar.Length)
            .GreaterThan(0).WithMessage("Avatar file cannot be empty.")
            .LessThanOrEqualTo(5 * 1024 * 1024).WithMessage("Avatar size cannot exceed 5 MB.");

        RuleFor(x => x.Request.Avatar.ContentType)
            .Must(contentType =>
                contentType == "image/jpeg" ||
                contentType == "image/png" ||
                contentType == "image/webp")
            .WithMessage("Only jpg, png and webp files are allowed.");
    }
}
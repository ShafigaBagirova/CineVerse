using Application.WatchRooms.Commands;
using FluentValidation;

namespace Application.Validations.WatchRooms;

public class CreateWatchRoomCommandValidator : AbstractValidator<CreateWatchRoomCommand>
{
    public CreateWatchRoomCommandValidator()
    {
        RuleFor(x => x.Request.ContentUrl)
            .NotEmpty().WithMessage("Content url boş ola bilməz.")
            .MaximumLength(2048).WithMessage("Content url maksimum 2048 simvol ola bilər.");

        RuleFor(x => x.Request.Title)
            .MaximumLength(200).WithMessage("Title maksimum 200 simvol ola bilər.");
    }
}

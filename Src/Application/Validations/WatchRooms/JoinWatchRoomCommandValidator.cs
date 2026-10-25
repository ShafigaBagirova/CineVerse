using Application.WatchRooms.Commands;
using FluentValidation;

namespace Application.Validations.WatchRooms;

public class JoinWatchRoomCommandValidator : AbstractValidator<JoinWatchRoomCommand>
{
    public JoinWatchRoomCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Room code boş ola bilməz.")
            .MaximumLength(16).WithMessage("Room code maksimum 16 simvol ola bilər.");
    }
}

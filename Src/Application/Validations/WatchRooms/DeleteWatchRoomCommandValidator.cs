using Application.WatchRooms.Commands;
using FluentValidation;

namespace Application.Validations.WatchRooms;

public class DeleteWatchRoomCommandValidator : AbstractValidator<DeleteWatchRoomCommand>
{
    public DeleteWatchRoomCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Watch room id must be greater than 0.");
    }
}

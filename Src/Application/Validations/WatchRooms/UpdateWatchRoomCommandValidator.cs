using Application.WatchRooms.Commands;
using FluentValidation;

namespace Application.Validations.WatchRooms;

public class UpdateWatchRoomCommandValidator : AbstractValidator<UpdateWatchRoomCommand>
{
    public UpdateWatchRoomCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Watch room id must be greater than 0.");

        RuleFor(x => x.Request.Title)
            .MaximumLength(200).WithMessage("Title maksimum 200 simvol ola bilər.");
    }
}

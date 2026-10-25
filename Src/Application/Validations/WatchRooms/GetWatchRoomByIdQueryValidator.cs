using Application.WatchRooms.Queries;
using FluentValidation;

namespace Application.Validations.WatchRooms;

public class GetWatchRoomByIdQueryValidator : AbstractValidator<GetWatchRoomByIdQuery>
{
    public GetWatchRoomByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Watch room id must be greater than 0.");
    }
}

using Application.WatchRooms.Queries;
using FluentValidation;

namespace Application.Validations.WatchRooms;

public class GetPopularYouTubeVideosQueryValidator : AbstractValidator<GetPopularYouTubeVideosQuery>
{
    public GetPopularYouTubeVideosQueryValidator()
    {
        RuleFor(x => x.PageToken)
            .MaximumLength(100).WithMessage("Page token maksimum 100 simvol ola bilər.")
            .Matches("^[A-Za-z0-9_-]*$").WithMessage("Page token yalnız hərf, rəqəm, '-' və '_' ola bilər.")
            .When(x => !string.IsNullOrEmpty(x.PageToken));
    }
}

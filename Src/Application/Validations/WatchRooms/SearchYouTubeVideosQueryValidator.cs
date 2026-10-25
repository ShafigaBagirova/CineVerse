using Application.WatchRooms.Queries;
using FluentValidation;

namespace Application.Validations.WatchRooms;

public class SearchYouTubeVideosQueryValidator : AbstractValidator<SearchYouTubeVideosQuery>
{
    public SearchYouTubeVideosQueryValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty().WithMessage("Search text boş ola bilməz.")
            .MaximumLength(100).WithMessage("Search text maksimum 100 simvol ola bilər.");
    }
}

using Application.Movies.Queries;
using FluentValidation;

namespace Application.Validations.Movie;

public sealed class GetMoviesByGenreQueryValidator
    : AbstractValidator<GetMoviesByGenreQuery>
{
    public GetMoviesByGenreQueryValidator()
    {
        RuleFor(x => x.GenreId)
            .GreaterThan(0)
            .WithMessage("GenreId must be greater than 0.");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("PageSize must be greater than 0.")
            .LessThanOrEqualTo(50)
            .WithMessage("PageSize must not exceed 50.");
    }
}

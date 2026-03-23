using Application.Movies.Queries;
using FluentValidation;

namespace Application.Validations.Movie;

public class GetAllMoviesQueryValidator : AbstractValidator<GetAllMoviesQuery>
{
    public GetAllMoviesQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("PageSize must be greater than 0.")
            .LessThanOrEqualTo(50).WithMessage("PageSize can be maximum 50.");

        RuleFor(x => x.GenreId)
            .GreaterThan(0)
            .When(x => x.GenreId.HasValue)
            .WithMessage("GenreId must be greater than 0.");

        RuleFor(x => x.Language)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.Language));


        RuleFor(x => x.Search)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Search));

        RuleFor(x => x.Year)
            .InclusiveBetween(1888, 2100)
            .When(x => x.Year.HasValue)
            .WithMessage("Year must be in the correct interval.");

        RuleFor(x => x.MinTmdbRating)
            .InclusiveBetween(0, 10)
            .When(x => x.MinTmdbRating.HasValue);

        RuleFor(x => x.MaxTmdbRating)
            .InclusiveBetween(0, 10)
            .When(x => x.MaxTmdbRating.HasValue);

        RuleFor(x => x.MinUserRating)
            .InclusiveBetween(0, 10)
            .When(x => x.MinUserRating.HasValue);

        RuleFor(x => x.MaxUserRating)
            .InclusiveBetween(0, 10)
            .When(x => x.MaxUserRating.HasValue);

        RuleFor(x => x)
            .Must(x => !x.MinTmdbRating.HasValue || !x.MaxTmdbRating.HasValue || x.MinTmdbRating <= x.MaxTmdbRating)
            .WithMessage("MinTmdbRating cannot be greater than MaxTmdbRating.");

        RuleFor(x => x)
            .Must(x => !x.MinUserRating.HasValue || !x.MaxUserRating.HasValue || x.MinUserRating <= x.MaxUserRating)
            .WithMessage("MinUserRating cannot be greater than MaxUserRating.");

        RuleFor(x => x.SortBy)
            .Must(x => string.IsNullOrWhiteSpace(x) ||
                       x.Trim().ToLower() is "title" or "year" or "tmdb_rating" or "user_rating" or "createdat")
            .WithMessage("SortBy can be only title,year,tmdb_rating,user_rating or createdat.");
    }
}

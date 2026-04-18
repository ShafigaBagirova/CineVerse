using Application.Movies.Queries;
using FluentValidation;

namespace Application.Validations.Movie;

public sealed class GetAllMoviesQueryValidator : AbstractValidator<GetAllMoviesQuery>
{
    public GetAllMoviesQueryValidator()
    {
        RuleFor(x => x.Request.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.Request.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("Page size must be between 1 and 100.");

        When(x => !string.IsNullOrWhiteSpace(x.Request.Search), () =>
        {
            RuleFor(x => x.Request.Search!)
                .MaximumLength(200)
                .WithMessage("Search must not exceed 200 characters.");
        });

        When(x => x.Request.GenreId.HasValue, () =>
        {
            RuleFor(x => x.Request.GenreId!.Value)
                .GreaterThan(0)
                .WithMessage("Genre id must be greater than 0.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Request.Language), () =>
        {
            RuleFor(x => x.Request.Language!)
                .MaximumLength(10)
                .WithMessage("Language must not exceed 10 characters.");
        });

        When(x => x.Request.Year.HasValue, () =>
        {
            RuleFor(x => x.Request.Year!.Value)
                .InclusiveBetween(1888, DateTime.UtcNow.Year)
                .WithMessage("Year must be valid.");
        });

        When(x => x.Request.MinTmdbRating.HasValue && x.Request.MaxTmdbRating.HasValue, () =>
        {
            RuleFor(x => x.Request)
                .Must(r => r.MinTmdbRating <= r.MaxTmdbRating)
                .WithMessage("MinTmdbRating must be less than or equal to MaxTmdbRating.");
        });

        When(x => x.Request.MinTmdbRating.HasValue, () =>
        {
            RuleFor(x => x.Request.MinTmdbRating!.Value)
                .InclusiveBetween(0, 10)
                .WithMessage("MinTmdbRating must be between 0 and 10.");
        });

        When(x => x.Request.MaxTmdbRating.HasValue, () =>
        {
            RuleFor(x => x.Request.MaxTmdbRating!.Value)
                .InclusiveBetween(0, 10)
                .WithMessage("MaxTmdbRating must be between 0 and 10.");
        });
        When(x => x.Request.MinUserRating.HasValue && x.Request.MaxUserRating.HasValue, () =>
        {
            RuleFor(x => x.Request)
                .Must(r => r.MinUserRating <= r.MaxUserRating)
                .WithMessage("MinUserRating must be less than or equal to MaxUserRating.");
        });

        When(x => x.Request.MinUserRating.HasValue, () =>
        {
            RuleFor(x => x.Request.MinUserRating!.Value)
                .InclusiveBetween(0, 10)
                .WithMessage("MinUserRating must be between 0 and 10.");
        });

        When(x => x.Request.MaxUserRating.HasValue, () =>
        {
            RuleFor(x => x.Request.MaxUserRating!.Value)
                .InclusiveBetween(0, 10)
                .WithMessage("MaxUserRating must be between 0 and 10.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Request.SortBy), () =>
        {
            RuleFor(x => x.Request.SortBy!)
                .Must(sortBy =>
                {
                    var key = sortBy.Trim().ToLowerInvariant();
                    return key is "title" or "year" or "tmdbrating" or "userrating" or "createdat";
                })
                .WithMessage("SortBy must be one of: title, year, tmdbRating, userRating, createdAt.");
        });
    }
}
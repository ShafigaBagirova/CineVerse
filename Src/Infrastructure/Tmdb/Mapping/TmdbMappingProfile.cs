using Application.Common.Helpers;
using Application.Movies.Dtos;
using AutoMapper;
using Domain.Enums;

namespace Infrastructure.Tmdb.Mapping;

public sealed class TmdbMappingProfile : Profile
{
    public TmdbMappingProfile()
    {
         CreateMap<TmdbMovieListItem, ExternalMovieDto>()
            .ForMember(dest => dest.ExternalId,
                opt => opt.MapFrom(src => src.Id))

            .ForMember(dest => dest.Description,
                opt => opt.MapFrom(src => src.Overview))

            .ForMember(dest => dest.ReleaseDate,
                opt => opt.MapFrom(src => ParseDateOnly(src.ReleaseDate)))

            .ForMember(dest => dest.Language,
                opt => opt.MapFrom(src => src.OriginalLanguage))

            .ForMember(dest => dest.TmdbRating,
                opt => opt.MapFrom(src => src.VoteAverage))

             .ForMember(dest => dest.PosterPath,
             opt => opt.MapFrom(src => src.PosterPath))

           .ForMember(dest => dest.BackdropPath,
             opt => opt.MapFrom(src => src.BackdropPath))

            .ForMember(dest => dest.Slug,
                opt => opt.MapFrom(src => SlugHelper.Generate(src.Title)))

            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => ResolveMovieStatus(src.ReleaseDate)))

            .ForMember(dest => dest.DurationMinutes, opt => opt.Ignore())
            .ForMember(dest => dest.Country, opt => opt.Ignore())
            .ForMember(dest => dest.AgeRating, opt => opt.Ignore())
            .ForMember(dest => dest.Tagline, opt => opt.Ignore())
            .ForMember(dest => dest.Director, opt => opt.Ignore());
    }

    private static DateOnly? ParseDateOnly(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return DateOnly.TryParse(value, out var parsed)
            ? parsed
            : null;
    }

    private static MovieStatus ResolveMovieStatus(string? releaseDateValue)
    {
        if (string.IsNullOrWhiteSpace(releaseDateValue))
            return MovieStatus.Upcoming;

        if (!DateOnly.TryParse(releaseDateValue, out var releaseDate))
            return MovieStatus.Upcoming;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return releaseDate > today
            ? MovieStatus.Upcoming
            : MovieStatus.Released;
    }
}

using Application.Movies.Dtos;
using AutoMapper;
using Domain.Entities;
using System.Linq;

namespace Application.Common.Mappings;

public class MovieProfile : Profile
{
    public MovieProfile()
    {
        CreateMap<ExternalMovieDto, Movie>()
     .ForMember(dest => dest.TmdbId, opt => opt.MapFrom(src => src.ExternalId))
     .ForMember(dest => dest.DurationMinutes, opt => opt.MapFrom(src => src.DurationMinutes))
     .ForMember(dest => dest.Slug,
        opt => opt.Ignore())
     .ForMember(dest => dest.RatingCount, opt => opt.Ignore())
     .ForMember(dest => dest.UserAverageRating, opt => opt.Ignore())
     .ForMember(dest => dest.ImdbRating, opt => opt.Ignore())
     .ForAllMembers(opt =>
         opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Movie, GetAllMoviesResponse>()
        .ForMember(
            d => d.TmdbId,
            opt => opt.MapFrom(src =>
                src.TmdbId.HasValue
                && src.TmdbId.Value >= int.MinValue
                && src.TmdbId.Value <= int.MaxValue
                    ? (int)src.TmdbId.Value
                    : 0))
        .ForMember(
            d => d.Status,
            opt => opt.MapFrom(src => src.Status.ToString()))
        .ForMember(
            d => d.ReleaseYear,
            opt => opt.MapFrom(src =>
                src.ReleaseDate.HasValue ? src.ReleaseDate.Value.Year : (int?)null))
        .ForMember(
            d => d.DurationMinutes,
            opt => opt.MapFrom(src => src.DurationMinutes ?? 0))
        .ForMember(
            d => d.PosterUrl,
            opt => opt.MapFrom(src =>
                src.PosterPath != null
                    ? "https://image.tmdb.org/t/p/w500" + src.PosterPath
                    : null))
        .ForMember(
            d => d.BackdropUrl,
            opt => opt.MapFrom(src =>
                src.BackdropPath != null
                    ? "https://image.tmdb.org/t/p/original" + src.BackdropPath
                    : null))
        .ForMember(
            d => d.Cast,
            opt => opt.MapFrom(src =>
                string.IsNullOrWhiteSpace(src.Actors)
                    ? new List<string>()
                    : src.Actors
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList()));

        CreateMap<MovieVideo, MovieVideoDto>();

        CreateMap<Movie, GetMovieByIdResponse>()
          .ForMember(d => d.Cast, opt => opt.Ignore())
          .ForMember(d => d.PosterUrl,
              opt => opt.MapFrom(src =>
                  src.PosterPath == null
                      ? null
                      : $"https://image.tmdb.org/t/p/w500{src.PosterPath}"))
            .ForMember(dest => dest.Genres,
        opt => opt.MapFrom(src => src.MovieGenres))
          .ForMember(dest => dest.Videos,
              opt => opt.MapFrom(src => src.Videos))
          .ForMember(d => d.BackdropUrl,
              opt => opt.MapFrom(src =>
                  src.BackdropPath == null
                      ? null
                      : $"https://image.tmdb.org/t/p/original{src.BackdropPath}"));
        CreateMap<CreateMovieRequest, Movie>()
       .ForMember(dest => dest.Slug, opt => opt.Ignore())
       .ForMember(dest => dest.Status, opt => opt.Ignore())
       .ForMember(dest => dest.ImdbRating, opt => opt.Ignore())
       .ForMember(dest => dest.TmdbRating, opt => opt.Ignore())
       .ForMember(dest => dest.UserAverageRating, opt => opt.Ignore())
       .ForMember(dest => dest.RatingCount, opt => opt.Ignore());

        CreateMap<UpdateMovieRequest, Movie>();

    }
}

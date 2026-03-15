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
                        opt => opt.MapFrom(src =>
                            string.IsNullOrWhiteSpace(src.ReleaseDate)
                                ? (DateOnly?)null
                                : DateOnly.Parse(src.ReleaseDate)))
                    .ForMember(dest => dest.Language,
                        opt => opt.MapFrom(src => src.OriginalLanguage))
                    .ForMember(dest => dest.TmdbRating,
                        opt => opt.MapFrom(src => src.VoteAverage))
                    .ForMember(dest => dest.Slug,
                        opt => opt.MapFrom(src => SlugHelper.Generate(src.Title)))
                    .ForMember(dest => dest.Status,
                        opt => opt.MapFrom(src =>
                            string.IsNullOrWhiteSpace(src.ReleaseDate)
                                ? MovieStatus.Upcoming
                                : DateOnly.Parse(src.ReleaseDate) <= DateOnly.FromDateTime(DateTime.UtcNow)
                                    ? MovieStatus.Released
                                    : MovieStatus.Upcoming));
    }
}

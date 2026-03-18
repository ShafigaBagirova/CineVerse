using Application.Movies.Dtos;
using AutoMapper;
using Domain.Entities;

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
        .ForMember(d => d.PosterUrl,
        opt => opt.MapFrom(src =>
            src.PosterPath == null
                ? null
                : $"https://image.tmdb.org/t/p/w500{src.PosterPath}"));

        CreateMap<Movie, GetMovieByIdResponse>()
          .ForMember(d => d.PosterUrl,
              opt => opt.MapFrom(src =>
                  src.PosterPath == null
                      ? null
                      : $"https://image.tmdb.org/t/p/w500{src.PosterPath}"))

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

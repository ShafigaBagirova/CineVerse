using Application.MoviePost.Dtos;
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
     .ForMember(dest => dest.DurationMinutes, opt => opt.MapFrom(src => src.DurationMinutes ?? 0))
     .ForMember(dest => dest.MediaItems, opt => opt.Ignore())
     .ForMember(dest => dest.RatingCount, opt => opt.Ignore())
     .ForMember(dest => dest.UserAverageRating, opt => opt.Ignore())
     .ForMember(dest => dest.ImdbRating, opt => opt.Ignore())
     .ForAllMembers(opt =>
         opt.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<Movie, GetAllMoviesResponse>()
            .ForMember(
                dest => dest.FirstMediaKey,
                opt => opt.MapFrom(src =>
                    src.MediaItems
                        .OrderBy(x => x.Order)
                        .Select(x => x.ObjectKey)
                        .FirstOrDefault()));

        CreateMap<Movie, GetMovieByIdResponse>()
                .ForMember(
                    dest => dest.FirstMediaKey,
                    opt => opt.MapFrom(src => src.MediaItems
                        .OrderBy(x => x.Id)
                        .Select(x => x.ObjectKey)
                        .FirstOrDefault()));
        CreateMap<CreateMovieRequest, Movie>()
       .ForMember(dest => dest.Slug, opt => opt.Ignore())
       .ForMember(dest => dest.Status, opt => opt.Ignore())
       .ForMember(dest => dest.ImdbRating, opt => opt.Ignore())
       .ForMember(dest => dest.TmdbRating, opt => opt.Ignore())
       .ForMember(dest => dest.UserAverageRating, opt => opt.Ignore())
       .ForMember(dest => dest.RatingCount, opt => opt.Ignore())
       .ForMember(dest => dest.MediaItems, opt => opt.Ignore());

        CreateMap<UpdateMovieRequest, Movie>();

    }
}

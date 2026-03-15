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
      .ForMember(x => x.TmdbId, opt => opt.MapFrom(x => x.ExternalId))
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
                dest => dest.MediaItems,
                opt => opt.MapFrom(src =>
                    src.MediaItems
                        .OrderBy(x => x.Order)));
        CreateMap<CreateMovieRequest, Movie>();

        CreateMap<UpdateMovieRequest, Movie>();

    }
}

using Application.Movies.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public class MovieProfile : Profile
{
    public MovieProfile()
    {
        CreateMap<MoviePoster, MoviePosterItemDto>();

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
    }
}

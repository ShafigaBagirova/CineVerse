using Application.Movies.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public class MovieGenreProfile:Profile
{
    public MovieGenreProfile()
    {
        CreateMap<MovieGenre, MovieGenreDto>()
    .ForMember(dest => dest.Name,
        opt => opt.MapFrom(src => src.Genre.Name));
    }
}

using Application.MoviePost.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public class MoviePosterProfile : Profile
{
    public MoviePosterProfile()
    {
        CreateMap<MoviePoster, MoviePosterItemDto>();

    }
}

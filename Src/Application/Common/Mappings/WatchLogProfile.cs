using Application.Watched.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public class WatchLogProfile:Profile
{
    public WatchLogProfile()
    {
        CreateMap<WatchLog, WatchedMovieDto>()
    .ForMember(dest => dest.MovieId,
        opt => opt.MapFrom(src => src.MovieId))
    .ForMember(dest => dest.Title,
        opt => opt.MapFrom(src => src.Movie.Title))
    .ForMember(dest => dest.PosterPath,
        opt => opt.MapFrom(src => src.Movie.PosterPath))
    .ForMember(dest => dest.UserAverageRating,
        opt => opt.MapFrom(src => src.Movie.UserAverageRating))
    .ForMember(dest => dest.CreatedAt,
        opt => opt.MapFrom(src => src.CreatedAt));
    }
}

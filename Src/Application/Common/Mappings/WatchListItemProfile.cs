using Application.WatchListItems.Dtos;
using Application.WatchListItems.Queries;
using AutoMapper;
using Domain.Entities;
using FluentValidation;

namespace Application.Common.Mappings;

public class WatchListItemProfile:Profile
{
    public WatchListItemProfile()
    {
        CreateMap<WatchListItem, WatchlistMovieDto>()
            .ForMember(dest => dest.MovieId,
                opt => opt.MapFrom(src => src.MovieId))
            .ForMember(dest => dest.Title,
                opt => opt.MapFrom(src => src.Movie.Title))
            .ForMember(dest => dest.PosterUrl,
                opt => opt.MapFrom(src => src.Movie.PosterPath))
            .ForMember(dest => dest.UserAverageRating,
                opt => opt.MapFrom(src => src.Movie.UserAverageRating))
            .ForMember(dest => dest.AddedAt,
                opt => opt.MapFrom(src => src.CreatedAt));
    }
}


using Application.Auth.User.Dtos;
using AutoMapper;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Mappings;

public class MovieRatingProfile:Profile
{
    public MovieRatingProfile()
    {
        CreateMap<MovieRating, UserRatingDto>()
         .ForMember(dest => dest.MovieTitle,
             opt => opt.MapFrom(src => src.Movie.Title))
         .ForMember(dest => dest.PosterUrl,
             opt => opt.MapFrom(src => src.Movie.PosterPath));
    }
}

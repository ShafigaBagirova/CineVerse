using Application.Reviews.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public class ReviewProfile:Profile
{
    public ReviewProfile() 
    {
        CreateMap<Review, ReviewDto>()
            .ForMember(d => d.MovieTitle, o => o.MapFrom(s => s.Movie != null ? s.Movie.Title : null))
            .ForMember(d => d.PosterPath, o => o.MapFrom(s => s.Movie != null ? s.Movie.PosterPath : null));
    }
}

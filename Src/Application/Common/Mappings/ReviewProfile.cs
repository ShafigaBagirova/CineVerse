using Application.Movies.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public class ReviewProfile:Profile
{
    public ReviewProfile() 
    {
        CreateMap<Review, ReviewDto>();
    }
}

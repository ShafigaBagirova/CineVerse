using Application.Reviews.Dtos;
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

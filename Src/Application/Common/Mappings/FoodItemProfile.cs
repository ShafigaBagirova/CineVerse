using Application.FoodItems.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public class FoodItemProfile:Profile
{
    public FoodItemProfile()
    {
        CreateMap<CreateFoodItemRequest, FoodItem>()
            .ForMember(dest => dest.ImageObjectKey, opt => opt.Ignore());

        CreateMap<UpdateFoodItemRequest, FoodItem>()
            .ForMember(dest => dest.ImageObjectKey, opt => opt.Ignore());
        CreateMap<FoodItem, FoodItemResponse>();
        CreateMap<FoodItem, FoodItemResponse>()
         .ForMember(dest => dest.FoodCategoryName,
        opt => opt.MapFrom(src => src.FoodCategory.Name))
        .ForMember(dest => dest.CinemaId,
        opt => opt.MapFrom(src => src.FoodCategory.CinemaId));

    }
}

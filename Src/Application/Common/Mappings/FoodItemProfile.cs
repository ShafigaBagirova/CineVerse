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
            .ForMember(dest => dest.ImageObjectKey, opt => opt.Ignore())
            .ForMember(dest => dest.FoodCategoryId, opt => opt.Ignore())
            .ForMember(dest => dest.CinemaId, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) =>
            {
                if (srcMember is null)
                    return false;

                if (srcMember is string str)
                    return !string.IsNullOrWhiteSpace(str);

                return true;
            }));

        CreateMap<FoodItem, FoodItemResponse>();
        CreateMap<FoodItem, FoodItemResponse>()
         .ForMember(dest => dest.FoodCategoryName,
        opt => opt.MapFrom(src => src.FoodCategory.Name))
        .ForMember(dest => dest.CinemaId,
        opt => opt.MapFrom(src => src.FoodCategory.CinemaId));

    }
}

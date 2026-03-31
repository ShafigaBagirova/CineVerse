using Application.FoodCategories.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public sealed class FoodCategoryProfile : Profile
{
    public FoodCategoryProfile()
    {
        CreateMap<CreateFoodCategoryRequest, FoodCategory>();

        CreateMap<UpdateFoodCategoryRequest, FoodCategory>()
           .ForMember(dest => dest.CinemaId, opt => opt.Ignore())
           .ForMember(dest => dest.DisplayOrder, opt => opt.Ignore())
           .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<FoodCategory, FoodCategoryResponse>();

        CreateMap<FoodCategory, FoodCategoryWithItemsResponse>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.FoodItems));
    }
}
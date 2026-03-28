using Application.FoodCategories.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public sealed class FoodCategoryMappingProfile : Profile
{
    public FoodCategoryMappingProfile()
    {
        CreateMap<CreateFoodCategoryRequest, FoodCategory>();

        CreateMap<UpdateFoodCategoryRequest, FoodCategory>();
    }
}
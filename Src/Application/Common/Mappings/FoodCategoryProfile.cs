using Application.FoodCategories.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public sealed class FoodCategoryProfile : Profile
{
    public FoodCategoryProfile()
    {
        CreateMap<CreateFoodCategoryRequest, FoodCategory>();

        CreateMap<UpdateFoodCategoryRequest, FoodCategory>();
    }
}
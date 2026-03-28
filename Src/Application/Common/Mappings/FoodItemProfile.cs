using Application.FoodItems.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public class FoodItemProfile:Profile
{
    public FoodItemProfile()
    {
        CreateMap<CreateFoodItemRequest, FoodItem>();
        CreateMap<UpdateFoodItemRequest, FoodItem>();
    }
}

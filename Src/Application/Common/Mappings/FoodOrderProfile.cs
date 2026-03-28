using Application.FoodOrders.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public class FoodOrderProfile:Profile
{
    public FoodOrderProfile()
    {
        CreateMap<FoodOrderItem, FoodOrderItemResponse>()
    .ForMember(dest => dest.FoodItemName,
        opt => opt.MapFrom(src => src.FoodItem.Name))
    .ForMember(dest => dest.TotalPrice,
        opt => opt.MapFrom(src => src.Quantity * src.UnitPrice));

        CreateMap<FoodOrder, FoodOrderResponse>()
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Items,
                opt => opt.MapFrom(src => src.FoodOrderItems));
    }
}

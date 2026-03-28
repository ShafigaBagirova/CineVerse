using Application.Common.Responses;
using Application.FoodOrders.Dtos;
using MediatR;

namespace Application.FoodOrders.Queries;

public record GetFoodOrderByIdQuery(int Id)
    : IRequest<BaseResponse<FoodOrderResponse>>;
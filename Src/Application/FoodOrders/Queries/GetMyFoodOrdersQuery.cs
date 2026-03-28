using Application.Common.Responses;
using Application.FoodOrders.Dtos;
using MediatR;

namespace Application.FoodOrders.Queries;

public record GetMyFoodOrdersQuery(GetMyFoodOrdersRequest Request)
    : IRequest<BaseResponse<List<FoodOrderResponse>>>;
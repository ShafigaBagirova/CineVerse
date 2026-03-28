using Application.Common.Responses;
using Application.FoodOrders.Dtos;
using MediatR;

namespace Application.FoodOrders.Queries;

public record GetFoodOrdersByDayQuery(GetFoodOrdersByDayRequest Request)
    : IRequest<BaseResponse<List<OrdersByDayResponse>>>;
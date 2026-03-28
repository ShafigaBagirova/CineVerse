using Application.Common.Responses;
using Application.FoodOrders.Dtos;
using MediatR;

namespace Application.FoodOrders.Queries;

public record GetFoodOrderSummaryQuery(GetFoodOrderSummaryRequest Request)
    : IRequest<BaseResponse<FoodOrderSummaryResponse>>;
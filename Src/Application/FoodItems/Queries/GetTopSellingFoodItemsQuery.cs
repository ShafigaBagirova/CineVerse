using Application.Common.Responses;
using Application.FoodItems.Dtos;
using MediatR;

namespace Application.FoodItems.Queries;

public record GetTopSellingFoodItemsQuery(int Take = 5)
    : IRequest<BaseResponse<List<TopSellingFoodItemResponse>>>;
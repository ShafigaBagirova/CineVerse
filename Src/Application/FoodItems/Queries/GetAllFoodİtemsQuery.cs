using Application.Common.Responses;
using Application.FoodItems.Dtos;
using MediatR;

namespace Application.FoodItems.Queries;

public record GetAllFoodItemsQuery(GetAllFoodItemsRequest Request)
    : IRequest<BaseResponse<List<FoodItemResponse>>>;

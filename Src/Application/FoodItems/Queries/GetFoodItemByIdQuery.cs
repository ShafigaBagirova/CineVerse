using Application.Common.Responses;
using Application.FoodItems.Dtos;
using MediatR;

namespace Application.FoodItems.Queries;

public record GetFoodItemByIdQuery(int Id)
    : IRequest<BaseResponse<FoodItemResponse>>;
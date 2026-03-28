using Application.Common.Responses;
using Application.FoodItems.Dtos;
using MediatR;

namespace Application.FoodItems.Commands;

public sealed record CreateFoodItemCommand(CreateFoodItemRequest Request)
    : IRequest<BaseResponse>;
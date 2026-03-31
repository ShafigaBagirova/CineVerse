using Application.Common.Responses;
using Application.FoodItems.Dtos;
using MediatR;

namespace Application.FoodItems.Commands;

public sealed record UpdateFoodItemImageCommand(int Id, UpdateFoodItemImageRequest Request)
    : IRequest<BaseResponse>;
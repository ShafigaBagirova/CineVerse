using Application.Common.Responses;
using MediatR;

namespace Application.FoodItems.Commands;

public sealed record DeleteFoodItemCommand(int Id)
    : IRequest<BaseResponse>;
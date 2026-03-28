using Application.Common.Responses;
using Application.FoodOrders.Dtos;
using MediatR;

namespace Application.FoodOrders.Commands;

public sealed record CreateFoodOrderDraftCommand(CreateFoodOrderDraftRequest Request)
    : IRequest<BaseResponse>;
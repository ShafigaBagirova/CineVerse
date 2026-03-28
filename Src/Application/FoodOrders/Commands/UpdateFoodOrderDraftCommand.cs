using Application.Common.Responses;
using Application.FoodOrders.Dtos;
using MediatR;

namespace Application.FoodOrders.Commands;

public sealed record UpdateFoodOrderDraftCommand(int Id, UpdateFoodOrderDraftRequest Request)
    : IRequest<BaseResponse>;
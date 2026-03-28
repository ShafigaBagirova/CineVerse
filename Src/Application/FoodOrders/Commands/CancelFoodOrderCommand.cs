using Application.Common.Responses;
using MediatR;

namespace Application.FoodOrders.Commands;

public sealed record CancelFoodOrderCommand(int Id): IRequest<BaseResponse>;
using Application.Common.Responses;
using MediatR;

namespace Application.FoodCategories.Commands;

public sealed record DeleteFoodCategoryCommand(int Id)
    : IRequest<BaseResponse>;
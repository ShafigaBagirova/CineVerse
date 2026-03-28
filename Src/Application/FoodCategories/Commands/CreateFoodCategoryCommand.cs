using Application.Common.Responses;
using Application.FoodCategories.Dtos;
using MediatR;

namespace Application.FoodCategories.Commands;

public sealed record CreateFoodCategoryCommand(CreateFoodCategoryRequest Request)
    : IRequest<BaseResponse>;
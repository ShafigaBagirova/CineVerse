using Application.Common.Responses;
using Application.FoodCategories.Dtos;
using MediatR;

namespace Application.FoodCategories.Queries;

public record GetFoodCategoryByIdQuery(int Id)
    : IRequest<BaseResponse<FoodCategoryResponse>>;
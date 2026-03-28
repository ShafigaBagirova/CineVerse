using Application.Common.Responses;
using Application.FoodCategories.Dtos;
using MediatR;

namespace Application.FoodCategories.Queries;

public record GetAllFoodCategoriesQuery(GetAllFoodCategoriesRequest Request)
    : IRequest<BaseResponse<List<FoodCategoryResponse>>>;

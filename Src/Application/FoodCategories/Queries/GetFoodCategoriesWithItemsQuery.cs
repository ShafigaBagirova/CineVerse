using Application.Common.Responses;
using Application.FoodCategories.Dtos;
using MediatR;

namespace Application.FoodCategories.Queries;

public record GetFoodCategoriesWithItemsQuery(GetFoodCategoriesWithItemsRequest Request)
    : IRequest<BaseResponse<List<FoodCategoryWithItemsResponse>>>;
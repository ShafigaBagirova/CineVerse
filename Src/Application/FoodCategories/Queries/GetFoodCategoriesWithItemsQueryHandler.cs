using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.FoodCategories.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.FoodCategories.Queries;

public class GetFoodCategoriesWithItemsQueryHandler
    : IRequestHandler<GetFoodCategoriesWithItemsQuery, BaseResponse<List<FoodCategoryWithItemsResponse>>>
{
    private readonly IFoodCategoryRepository _foodCategoryRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetFoodCategoriesWithItemsQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetFoodCategoriesWithItemsQueryHandler(
        IFoodCategoryRepository foodCategoryRepository,
        IMapper mapper,
        ILogger<GetFoodCategoriesWithItemsQueryHandler> logger,
        ICacheService cacheService)
    {
        _foodCategoryRepository = foodCategoryRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<List<FoodCategoryWithItemsResponse>>> Handle(
        GetFoodCategoriesWithItemsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "GetFoodCategoriesWithItemsQuery started. CinemaId: {CinemaId}, IsActive: {IsActive}, Search: {Search}, OnlyAvailableItems: {OnlyAvailableItems}",
            request.Request.CinemaId,
            request.Request.IsActive,
            request.Request.Search,
            request.Request.OnlyAvailableItems);

        var cacheKey =
            $"foodcategories:withitems:" +
            $"cinema:{request.Request.CinemaId?.ToString() ?? "null"}:" +
            $"active:{request.Request.IsActive?.ToString() ?? "null"}:" +
            $"search:{request.Request.Search?.Trim().ToLower() ?? "null"}:" +
            $"availableOnly:{request.Request.OnlyAvailableItems}";

        var cachedData =
            await _cacheService.GetAsync<List<FoodCategoryWithItemsResponse>>(cacheKey);

        if (cachedData is not null)
        {
            _logger.LogInformation(
                "Food categories with items retrieved from cache. CacheKey: {CacheKey}",
                cacheKey);

            return new BaseResponse<List<FoodCategoryWithItemsResponse>>
            {
                Success = true,
                Message = "Food categories with items retrieved successfully from cache.",
                Data = cachedData
            };
        }

        var categories = await _foodCategoryRepository.GetCategoriesWithItemsAsync(
            request.Request.CinemaId,
            request.Request.IsActive,
            request.Request.Search,
            cancellationToken);
       
        var response = _mapper.Map<List<FoodCategoryWithItemsResponse>>(categories);
          response = response
            .Where(c => c.Items != null && c.Items.Any())
             .ToList();
        foreach (var category in response)
        {
            category.Items = request.Request.SortBy switch
            {
                "price" => category.Items.OrderBy(x => x.Price).ToList(),
                _ => category.Items.OrderBy(x => x.Name).ToList()
            };
        }

        if (request.Request.OnlyAvailableItems)
        { 
         
            response = response
                .Select(category =>
                {
                    category.Items = category.Items
                        .Where(item => item.IsAvailable)
                        .ToList();

                    return category;
                })
                .Where(category => category.Items.Any())
                .ToList();
        }
        
        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

        _logger.LogInformation(
            "GetFoodCategoriesWithItemsQuery completed successfully. CategoryCount: {CategoryCount}",
            response.Count);

        return new BaseResponse<List<FoodCategoryWithItemsResponse>>
        {
            Success = true,
            Message = "Food categories with items retrieved successfully.",
            Data = response
        };
    }
}
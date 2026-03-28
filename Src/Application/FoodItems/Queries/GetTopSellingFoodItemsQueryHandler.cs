using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.FoodItems.Dtos;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Application.FoodItems.Queries;

public class GetTopSellingFoodItemsQueryHandler
    : IRequestHandler<GetTopSellingFoodItemsQuery, BaseResponse<List<TopSellingFoodItemResponse>>>
{
    private readonly IFoodOrderItemRepository _repository;
    private readonly ILogger<GetTopSellingFoodItemsQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetTopSellingFoodItemsQueryHandler(
        IFoodOrderItemRepository repository,
        ILogger<GetTopSellingFoodItemsQueryHandler> logger,
        ICacheService cacheService)
    {
        _repository = repository;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<List<TopSellingFoodItemResponse>>> Handle(
        GetTopSellingFoodItemsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "GetTopSellingFoodItemsQuery started. Take: {Take}",
            request.Take);

        var cacheKey = $"foodorders:top-selling:take:{request.Take}";

        var cachedData =
            await _cacheService.GetAsync<List<TopSellingFoodItemResponse>>(cacheKey);

        if (cachedData is not null)
        {
            _logger.LogInformation(
                "Top selling food items retrieved from cache. CacheKey: {CacheKey}",
                cacheKey);

            return new BaseResponse<List<TopSellingFoodItemResponse>>
            {
                Success = true,
                Message = "Top selling food items retrieved successfully from cache.",
                Data = cachedData
            };
        }

        var query = _repository.GetQueryable();

        var result = await query
            .Where(x => x.FoodOrder.Status == FoodOrderStatus.Confirmed ||
                        x.FoodOrder.Status == FoodOrderStatus.Delivered)
            .GroupBy(x => new { x.FoodItemId, x.FoodItem.Name })
            .Select(g => new TopSellingFoodItemResponse
            {
                FoodItemId = g.Key.FoodItemId,
                FoodItemName = g.Key.Name,
                TotalQuantity = g.Sum(x => x.Quantity)
            })
            .OrderByDescending(x => x.TotalQuantity)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(10));

        _logger.LogInformation(
            "GetTopSellingFoodItemsQuery completed successfully. Count: {Count}",
            result.Count);

        return new BaseResponse<List<TopSellingFoodItemResponse>>
        {
            Success = true,
            Message = "Top selling food items retrieved successfully.",
            Data = result
        };
    }
}
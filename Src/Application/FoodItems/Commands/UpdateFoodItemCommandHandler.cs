using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.FoodItems.Commands;

public sealed class UpdateFoodItemCommandHandler
    : IRequestHandler<UpdateFoodItemCommand, BaseResponse>
{
    private readonly IFoodItemRepository _foodItemRepository;
    private readonly IFoodCategoryRepository _foodCategoryRepository;
    private readonly ILogger<UpdateFoodItemCommandHandler> _logger;
    private readonly ICacheService _cacheService;

    public UpdateFoodItemCommandHandler(
        IFoodItemRepository foodItemRepository,
        IFoodCategoryRepository foodCategoryRepository,
        ILogger<UpdateFoodItemCommandHandler> logger,
        ICacheService cacheService)
    {
        _foodItemRepository = foodItemRepository;
        _foodCategoryRepository = foodCategoryRepository;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse> Handle(
        UpdateFoodItemCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "UpdateFoodItemCommand started. FoodItemId: {FoodItemId}",
            request.Id);

        var item = await _foodItemRepository.GetByIdAsync(request.Id, cancellationToken);

        if (item is null)
        {
            _logger.LogWarning(
                "UpdateFoodItemCommand failed. Food item not found. FoodItemId: {FoodItemId}",
                request.Id);

            return BaseResponse.Fail("Food item not found.");
        }

        var req = request.Request;

        if (req.FoodCategoryId.HasValue && req.FoodCategoryId.Value > 0)
        {
            var category = await _foodCategoryRepository.GetByIdAsync(
                req.FoodCategoryId.Value,
                cancellationToken);

            if (category is null || !category.IsActive)
                return BaseResponse.Fail("Food category not found.");

            item.FoodCategoryId = category.Id;
            item.CinemaId = category.CinemaId;
        }

        // Partial update: only apply fields present in the request (omit/null = leave DB value unchanged).
        if (req.Name is not null)
            item.Name = req.Name.Trim();

        if (req.Description is not null)
            item.Description = string.IsNullOrWhiteSpace(req.Description) ? null : req.Description.Trim();

        if (req.Price.HasValue)
            item.Price = req.Price.Value;

        if (req.IsAvailable.HasValue)
            item.IsAvailable = req.IsAvailable.Value;

        if (req.IsActive.HasValue)
            item.IsActive = req.IsActive.Value;

        await _foodItemRepository.UpdateAsync(item, cancellationToken);
        await _foodItemRepository.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync(FoodItemCacheKey.GetById(item.Id), cancellationToken);
        await _cacheService.RemoveByPrefixAsync(FoodItemCacheKey.AllPrefix);
        await _cacheService.RemoveByPrefixAsync(FoodCategoryCacheKey.WithItemsPrefix);
        await _cacheService.RemoveByPrefixAsync(FoodOrderCacheKey.TopSellingPrefix);

        _logger.LogInformation(
            "UpdateFoodItemCommand completed successfully. FoodItemId: {FoodItemId}",
            item.Id);

        return BaseResponse.Ok("Food item updated successfully.");
    }
}
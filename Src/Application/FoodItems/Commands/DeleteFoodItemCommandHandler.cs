using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.FoodItems.Commands;

public sealed class DeleteFoodItemCommandHandler
    : IRequestHandler<DeleteFoodItemCommand, BaseResponse>
{
    private readonly IFoodItemRepository _foodItemRepository;
    private readonly ILogger<DeleteFoodItemCommandHandler> _logger;
    private readonly ICacheService _cacheService;

    public DeleteFoodItemCommandHandler(
        IFoodItemRepository foodItemRepository,
        ILogger<DeleteFoodItemCommandHandler> logger,
        ICacheService cacheService)
    {
        _foodItemRepository = foodItemRepository;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse> Handle(
        DeleteFoodItemCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "DeleteFoodItemCommand started. FoodItemId: {FoodItemId}",
            request.Id);

        var item = await _foodItemRepository.GetByIdAsync(request.Id, cancellationToken);

        if (item is null)
        {
            _logger.LogWarning(
                "DeleteFoodItemCommand failed. Food item not found. FoodItemId: {FoodItemId}",
                request.Id);

            return BaseResponse.Fail("Food item not found.");
        }

        item.IsActive = false;

        await _foodItemRepository.UpdateAsync(item, cancellationToken);
        await _foodItemRepository.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync(FoodItemCacheKey.GetById(item.Id), cancellationToken);
        await _cacheService.RemoveByPrefixAsync(FoodItemCacheKey.AllPrefix);
        await _cacheService.RemoveByPrefixAsync(FoodCategoryCacheKey.WithItemsPrefix);
        await _cacheService.RemoveByPrefixAsync(FoodOrderCacheKey.TopSellingPrefix);
        _logger.LogInformation(
            "DeleteFoodItemCommand completed successfully. FoodItemId: {FoodItemId}",
            item.Id);

        return BaseResponse.Ok("Food item deleted successfully.");
    }
}
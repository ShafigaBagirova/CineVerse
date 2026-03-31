using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.FoodItems.Commands;

public sealed class UpdateFoodItemImageCommandHandler
    : IRequestHandler<UpdateFoodItemImageCommand, BaseResponse>
{
    private readonly IFoodItemRepository _foodItemRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<UpdateFoodItemImageCommandHandler> _logger;
    private readonly ICacheService _cacheService;

    public UpdateFoodItemImageCommandHandler(
        IFoodItemRepository foodItemRepository,
        IFileStorageService fileStorageService,
        ILogger<UpdateFoodItemImageCommandHandler> logger,
        ICacheService cacheService)
    {
        _foodItemRepository = foodItemRepository;
        _fileStorageService = fileStorageService;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse> Handle(
        UpdateFoodItemImageCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "UpdateFoodItemImageCommand started. FoodItemId: {FoodItemId}",
            request.Id);

        var item = await _foodItemRepository.GetByIdAsync(request.Id, cancellationToken);

        if (item is null)
        {
            _logger.LogWarning(
                "UpdateFoodItemImageCommand failed. Food item not found. FoodItemId: {FoodItemId}",
                request.Id);

            return BaseResponse.Fail("Food item not found.");
        }

        if (request.Request.Image is null || request.Request.Image.Length == 0)
            return BaseResponse.Fail("Image is required.");

        var oldImageObjectKey = item.ImageObjectKey;

        await using var stream = request.Request.Image.OpenReadStream();

        item.ImageObjectKey = await _fileStorageService.SaveAsync(
            stream,
            request.Request.Image.FileName,
            request.Request.Image.ContentType,
            "food-items",
            cancellationToken);

        await _foodItemRepository.UpdateAsync(item, cancellationToken);
        await _foodItemRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync(FoodItemCacheKey.GetById(item.Id), cancellationToken);
        await _cacheService.RemoveByPrefixAsync(FoodItemCacheKey.AllPrefix);
        await _cacheService.RemoveByPrefixAsync(FoodCategoryCacheKey.WithItemsPrefix);
        await _cacheService.RemoveByPrefixAsync(FoodOrderCacheKey.TopSellingPrefix);

        if (!string.IsNullOrWhiteSpace(oldImageObjectKey) &&
            oldImageObjectKey != item.ImageObjectKey)
        {
            try
            {
                await _fileStorageService.DeleteFileAsync(oldImageObjectKey, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Old food item image delete failed. FoodItemId: {FoodItemId}, ObjectKey: {ObjectKey}",
                    item.Id,
                    oldImageObjectKey);
            }
        }

        _logger.LogInformation(
            "UpdateFoodItemImageCommand completed successfully. FoodItemId: {FoodItemId}",
            item.Id);

        return BaseResponse.Ok("Food item image updated successfully.");
    }
}
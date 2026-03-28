using Application.Common.Interfaces;
using Application.Common.Responses;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.FoodItems.Commands;

public sealed class UpdateFoodItemCommandHandler
    : IRequestHandler<UpdateFoodItemCommand, BaseResponse>
{
    private readonly IFoodItemRepository _foodItemRepository;
    private readonly IFoodCategoryRepository _foodCategoryRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateFoodItemCommandHandler> _logger;
    private readonly ICacheService _cacheService;

    public UpdateFoodItemCommandHandler(
        IFoodItemRepository foodItemRepository,
        IFoodCategoryRepository foodCategoryRepository,
        IFileStorageService fileStorageService,
        IMapper mapper,
        ILogger<UpdateFoodItemCommandHandler> logger,
        ICacheService cacheService)
    {
        _foodItemRepository = foodItemRepository;
        _foodCategoryRepository = foodCategoryRepository;
        _fileStorageService = fileStorageService;
        _mapper = mapper;
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

        var category = await _foodCategoryRepository.GetByIdAsync(
            request.Request.FoodCategoryId,
            cancellationToken);

        if (category is null || !category.IsActive)
        {
            _logger.LogWarning(
                "UpdateFoodItemCommand failed. Food category not found or inactive. FoodCategoryId: {FoodCategoryId}",
                request.Request.FoodCategoryId);

            return BaseResponse.Fail("Food category not found.");
        }

        var oldImageObjectKey = item.ImageObjectKey;

        _mapper.Map(request.Request, item);
        item.Name = item.Name.Trim();

        if (request.Request.Image is not null && request.Request.Image.Length > 0)
        {
            await using var stream = request.Request.Image.OpenReadStream();

            item.ImageObjectKey = await _fileStorageService.SaveAsync(
                stream,
                request.Request.Image.FileName,
                request.Request.Image.ContentType,
                "food-items",
                cancellationToken);
        }

        await _foodItemRepository.UpdateAsync(item, cancellationToken);
        await _foodItemRepository.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync($"fooditem:{item.Id}");
        await _cacheService.RemoveByPrefixAsync("fooditems:");
        await _cacheService.RemoveByPrefixAsync("foodcategories:withitems:");
        await _cacheService.RemoveByPrefixAsync("foodorders:top-selling:");

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
            "UpdateFoodItemCommand completed successfully. FoodItemId: {FoodItemId}",
            item.Id);

        return BaseResponse.Ok("Food item updated successfully.");
    }
}
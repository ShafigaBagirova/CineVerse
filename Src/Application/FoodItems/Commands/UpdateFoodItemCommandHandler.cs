using Application.Common.Helpers;
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

        if (request.Request.FoodCategoryId.HasValue && request.Request.FoodCategoryId.Value > 0)
        {
            var category = await _foodCategoryRepository.GetByIdAsync(
                request.Request.FoodCategoryId.Value,
                cancellationToken);

            if (category is null || !category.IsActive)
                return BaseResponse.Fail("Food category not found.");

            item.FoodCategoryId = category.Id;
            item.CinemaId = category.CinemaId;
        }


        _mapper.Map(request.Request, item);
        if (!string.IsNullOrWhiteSpace(item.Name))
            item.Name = item.Name.Trim();

        if (!string.IsNullOrWhiteSpace(item.Description))
            item.Description = item.Description.Trim();


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
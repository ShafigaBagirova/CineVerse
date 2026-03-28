using Application.Common.Interfaces;
using Application.Common.Responses;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.FoodItems.Commands;

public sealed class UpdateFoodItemCommandHandler
    : IRequestHandler<UpdateFoodItemCommand, BaseResponse>
{
    private readonly IFoodItemRepository _foodItemRepository;
    private readonly IFoodCategoryRepository _foodCategoryRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateFoodItemCommandHandler> _logger;

    public UpdateFoodItemCommandHandler(
        IFoodItemRepository foodItemRepository,
        IFoodCategoryRepository foodCategoryRepository,
        IMapper mapper,
        ILogger<UpdateFoodItemCommandHandler> logger)
    {
        _foodItemRepository = foodItemRepository;
        _foodCategoryRepository = foodCategoryRepository;
        _mapper = mapper;
        _logger = logger;
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

        _mapper.Map(request.Request, item);

        item.Name = item.Name.Trim();

        await _foodItemRepository.UpdateAsync(item, cancellationToken);
        await _foodItemRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "UpdateFoodItemCommand completed successfully. FoodItemId: {FoodItemId}",
            item.Id);

        return BaseResponse.Ok("Food item updated successfully.");
    }
}
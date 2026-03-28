using Application.Common.Interfaces;
using Application.Common.Responses;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.FoodItems.Commands;

public sealed class CreateFoodItemCommandHandler
    : IRequestHandler<CreateFoodItemCommand, BaseResponse>
{
    private readonly IFoodItemRepository _foodItemRepository;
    private readonly IFoodCategoryRepository _foodCategoryRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateFoodItemCommandHandler> _logger;

    public CreateFoodItemCommandHandler(
        IFoodItemRepository foodItemRepository,
        IFoodCategoryRepository foodCategoryRepository,
        IMapper mapper,
        ILogger<CreateFoodItemCommandHandler> logger)
    {
        _foodItemRepository = foodItemRepository;
        _foodCategoryRepository = foodCategoryRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<BaseResponse> Handle(
        CreateFoodItemCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "CreateFoodItemCommand started. Name: {Name}, FoodCategoryId: {FoodCategoryId}",
            request.Request.Name,
            request.Request.FoodCategoryId);

        var category = await _foodCategoryRepository.GetByIdAsync(
            request.Request.FoodCategoryId,
            cancellationToken);

        if (category is null || !category.IsActive)
        {
            _logger.LogWarning(
                "CreateFoodItemCommand failed. Food category not found or inactive. FoodCategoryId: {FoodCategoryId}",
                request.Request.FoodCategoryId);

            return BaseResponse.Fail("Food category not found.");
        }

        var entity = _mapper.Map<FoodItem>(request.Request);

        entity.Name = entity.Name.Trim();
        entity.IsActive = true;

        await _foodItemRepository.AddAsync(entity, cancellationToken);
        await _foodItemRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "CreateFoodItemCommand completed successfully. FoodItemId: {FoodItemId}",
            entity.Id);

        return BaseResponse.Ok("Food item created successfully.");
    }
}
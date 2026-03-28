using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.FoodItems.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.FoodItems.Queries;

public class GetFoodItemByIdQueryHandler
    : IRequestHandler<GetFoodItemByIdQuery, BaseResponse<FoodItemResponse>>
{
    private readonly IFoodItemRepository _foodItemRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetFoodItemByIdQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetFoodItemByIdQueryHandler(
        IFoodItemRepository foodItemRepository,
        IMapper mapper,
        ILogger<GetFoodItemByIdQueryHandler> logger,
        ICacheService cacheService)
    {
        _foodItemRepository = foodItemRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<FoodItemResponse>> Handle(
        GetFoodItemByIdQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetFoodItemByIdQuery started for Id: {Id}", request.Id);

        var cacheKey = $"fooditem:{request.Id}";
        var cachedData = await _cacheService.GetAsync<FoodItemResponse>(cacheKey);

        if (cachedData is not null)
        {
            _logger.LogInformation("Food item retrieved from cache for Id: {Id}", request.Id);

            return new BaseResponse<FoodItemResponse>
            {
                Success = true,
                Message = "Food item retrieved successfully from cache.",
                Data = cachedData
            };
        }

        var foodItem = await _foodItemRepository.GetByIdAsync(request.Id, cancellationToken);

        if (foodItem is null)
        {
            _logger.LogWarning("Food item not found for Id: {Id}", request.Id);

            return new BaseResponse<FoodItemResponse>
            {
                Success = false,
                Message = "Food item not found."
            };
        }

        var response = _mapper.Map<FoodItemResponse>(foodItem);

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

        _logger.LogInformation("GetFoodItemByIdQuery completed successfully for Id: {Id}", request.Id);

        return new BaseResponse<FoodItemResponse>
        {
            Success = true,
            Message = "Food item retrieved successfully.",
            Data = response
        };
    }
}
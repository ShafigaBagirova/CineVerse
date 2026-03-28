using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.FoodCategories.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.FoodCategories.Queries;

public class GetFoodCategoryByIdQueryHandler
    : IRequestHandler<GetFoodCategoryByIdQuery, BaseResponse<FoodCategoryResponse>>
{
    private readonly IFoodCategoryRepository _foodCategoryRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetFoodCategoryByIdQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetFoodCategoryByIdQueryHandler(
        IFoodCategoryRepository foodCategoryRepository,
        IMapper mapper,
        ILogger<GetFoodCategoryByIdQueryHandler> logger,
        ICacheService cacheService)
    {
        _foodCategoryRepository = foodCategoryRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<FoodCategoryResponse>> Handle(
        GetFoodCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetFoodCategoryByIdQuery started for Id: {Id}", request.Id);

        var cacheKey = $"foodcategory:{request.Id}";
        var cachedData = await _cacheService.GetAsync<FoodCategoryResponse>(cacheKey);

        if (cachedData is not null)
        {
            _logger.LogInformation("Food category found in cache for Id: {Id}", request.Id);

            return new BaseResponse<FoodCategoryResponse>
            {
                Success = true,
                Message = "Food category retrieved successfully from cache.",
                Data = cachedData
            };
        }

        var foodCategory = await _foodCategoryRepository.GetByIdAsync(request.Id, cancellationToken);

        if (foodCategory is null)
        {
            _logger.LogWarning("Food category not found for Id: {Id}", request.Id);

            return new BaseResponse<FoodCategoryResponse>
            {
                Success = false,
                Message = "Food category not found."
            };
        }

        var response = _mapper.Map<FoodCategoryResponse>(foodCategory);

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

        _logger.LogInformation("Food category retrieved successfully for Id: {Id}", request.Id);

        return new BaseResponse<FoodCategoryResponse>
        {
            Success = true,
            Message = "Food category retrieved successfully.",
            Data = response
        };
    }
}
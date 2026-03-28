using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.FoodItems.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.FoodItems.Queries;

public class GetAllFoodItemsQueryHandler
    : IRequestHandler<GetAllFoodItemsQuery, BaseResponse<List<FoodItemResponse>>>
{
    private readonly IFoodItemRepository _foodItemRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllFoodItemsQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetAllFoodItemsQueryHandler(
        IFoodItemRepository foodItemRepository,
        IMapper mapper,
        ILogger<GetAllFoodItemsQueryHandler> logger,
        ICacheService cacheService)
    {
        _foodItemRepository = foodItemRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<List<FoodItemResponse>>> Handle(
        GetAllFoodItemsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "GetAllFoodItemsQuery started. CinemaId: {CinemaId}, FoodCategoryId: {FoodCategoryId}, IsAvailable: {IsAvailable}, Search: {Search}, MinPrice: {MinPrice}, MaxPrice: {MaxPrice}, PageNumber: {PageNumber}, PageSize: {PageSize}",
            request.Request.CinemaId,
            request.Request.FoodCategoryId,
            request.Request.IsAvailable,
            request.Request.Search,
            request.Request.MinPrice,
            request.Request.MaxPrice,
            request.Request.PageNumber,
            request.Request.PageSize);

        var cacheKey =
            $"fooditems:" +
            $"cinema:{request.Request.CinemaId?.ToString() ?? "null"}:" +
            $"category:{request.Request.FoodCategoryId?.ToString() ?? "null"}:" +
            $"available:{request.Request.IsAvailable?.ToString() ?? "null"}:" +
            $"search:{request.Request.Search?.Trim().ToLower() ?? "null"}:" +
            $"min:{request.Request.MinPrice?.ToString() ?? "null"}:" +
            $"max:{request.Request.MaxPrice?.ToString() ?? "null"}:" +
            $"page:{request.Request.PageNumber}:" +
            $"size:{request.Request.PageSize}";

        var cachedData = await _cacheService.GetAsync<List<FoodItemResponse>>(cacheKey);

        if (cachedData is not null)
        {
            _logger.LogInformation("Food items retrieved from cache. CacheKey: {CacheKey}", cacheKey);

            return new BaseResponse<List<FoodItemResponse>>
            {
                Success = true,
                Message = "Food items retrieved successfully from cache.",
                Data = cachedData
            };
        }

        var query = await _foodItemRepository.GetQueryableAsync();

        if (request.Request.CinemaId.HasValue)
        {
            query = query.Where(x => x.FoodCategory.CinemaId == request.Request.CinemaId.Value);
        }

        if (request.Request.FoodCategoryId.HasValue)
        {
            query = query.Where(x => x.FoodCategoryId == request.Request.FoodCategoryId.Value);
        }

        if (request.Request.IsAvailable.HasValue)
        {
            query = query.Where(x => x.IsAvailable == request.Request.IsAvailable.Value);
        }

        if (request.Request.MinPrice.HasValue)
        {
            query = query.Where(x => x.Price >= request.Request.MinPrice.Value);
        }

        if (request.Request.MaxPrice.HasValue)
        {
            query = query.Where(x => x.Price <= request.Request.MaxPrice.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
        {
            var search = request.Request.Search.Trim().ToLower();

            query = query.Where(x =>
                x.Name.ToLower().Contains(search) ||
                (x.Description != null && x.Description.ToLower().Contains(search)));
        }

        query = query
            .OrderBy(x => x.Name)
            .Skip((request.Request.PageNumber - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize);

        var foodItems = await _foodItemRepository.ToListAsync(query, cancellationToken);

        var response = _mapper.Map<List<FoodItemResponse>>(foodItems);

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

        _logger.LogInformation(
            "GetAllFoodItemsQuery completed successfully. ReturnedCount: {Count}",
            response.Count);

        return new BaseResponse<List<FoodItemResponse>>
        {
            Success = true,
            Message = "Food items retrieved successfully.",
            Data = response
        };
    }
}
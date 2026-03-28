using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.FoodCategories.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.FoodCategories.Queries;

public class GetAllFoodCategoriesQueryHandler
    : IRequestHandler<GetAllFoodCategoriesQuery, BaseResponse<List<FoodCategoryResponse>>>
{
    private readonly IFoodCategoryRepository _foodCategoryRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllFoodCategoriesQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetAllFoodCategoriesQueryHandler(
        IFoodCategoryRepository foodCategoryRepository,
        IMapper mapper,
        ILogger<GetAllFoodCategoriesQueryHandler> logger,
        ICacheService cacheService)
    {
        _foodCategoryRepository = foodCategoryRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<List<FoodCategoryResponse>>> Handle(
        GetAllFoodCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "GetAllFoodCategoriesQuery started. CinemaId: {CinemaId}, IsActive: {IsActive}, Search: {Search}, PageNumber: {PageNumber}, PageSize: {PageSize}",
            request.Request.CinemaId,
            request.Request.IsActive,
            request.Request.Search,
            request.Request.PageNumber,
            request.Request.PageSize);

        var cacheKey =
            $"foodcategories:cinema:{request.Request.CinemaId?.ToString() ?? "null"}:" +
            $"active:{request.Request.IsActive?.ToString() ?? "null"}:" +
            $"search:{request.Request.Search?.Trim().ToLower() ?? "null"}:" +
            $"page:{request.Request.PageNumber}:size:{request.Request.PageSize}";

        var cachedData = await _cacheService.GetAsync<List<FoodCategoryResponse>>(cacheKey);

        if (cachedData is not null)
        {
            _logger.LogInformation("Food categories retrieved from cache. CacheKey: {CacheKey}", cacheKey);

            return new BaseResponse<List<FoodCategoryResponse>>
            {
                Success = true,
                Message = "Food categories retrieved successfully from cache.",
                Data = cachedData
            };
        }

        var query = await _foodCategoryRepository.GetQueryableAsync();

        if (request.Request.CinemaId.HasValue)
        {
            query = query.Where(x => x.CinemaId == request.Request.CinemaId.Value);
        }

        if (request.Request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.Request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Request.Search))
        {
            var search = request.Request.Search.Trim().ToLower();
            query = query.Where(x =>
                x.Name.ToLower().Contains(search) ||
                (x.Description != null && x.Description.ToLower().Contains(search)));
        }

        query = query
            .OrderBy(x => x.Id)
            .Skip((request.Request.PageNumber - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize);

        var foodCategories = await _foodCategoryRepository.ToListAsync(query, cancellationToken);

        var response = _mapper.Map<List<FoodCategoryResponse>>(foodCategories);

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

        _logger.LogInformation(
            "GetAllFoodCategoriesQuery completed successfully. ReturnedCount: {Count}",
            response.Count);

        return new BaseResponse<List<FoodCategoryResponse>>
        {
            Success = true,
            Message = "Food categories retrieved successfully.",
            Data = response
        };
    }
}
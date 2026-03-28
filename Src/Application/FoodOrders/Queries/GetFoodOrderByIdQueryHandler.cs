using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.FoodOrders.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.FoodOrders.Queries;

public class GetFoodOrderByIdQueryHandler
    : IRequestHandler<GetFoodOrderByIdQuery, BaseResponse<FoodOrderResponse>>
{
    private readonly IFoodOrderRepository _foodOrderRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetFoodOrderByIdQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetFoodOrderByIdQueryHandler(
        IFoodOrderRepository foodOrderRepository,
        IMapper mapper,
        ILogger<GetFoodOrderByIdQueryHandler> logger,
        ICacheService cacheService)
    {
        _foodOrderRepository = foodOrderRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<FoodOrderResponse>> Handle(
        GetFoodOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetFoodOrderByIdQuery started for Id: {Id}", request.Id);

        var cacheKey = $"foodorder:{request.Id}";
        var cachedData = await _cacheService.GetAsync<FoodOrderResponse>(cacheKey);

        if (cachedData is not null)
        {
            _logger.LogInformation("Food order retrieved from cache for Id: {Id}", request.Id);

            return new BaseResponse<FoodOrderResponse>
            {
                Success = true,
                Message = "Food order retrieved successfully from cache.",
                Data = cachedData
            };
        }

        var order = await _foodOrderRepository.GetByIdAsync(request.Id, cancellationToken);

        if (order is null)
        {
            _logger.LogWarning("Food order not found for Id: {Id}", request.Id);

            return new BaseResponse<FoodOrderResponse>
            {
                Success = false,
                Message = "Food order not found."
            };
        }

        var response = _mapper.Map<FoodOrderResponse>(order);

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));

        _logger.LogInformation("GetFoodOrderByIdQuery completed successfully for Id: {Id}", request.Id);

        return new BaseResponse<FoodOrderResponse>
        {
            Success = true,
            Message = "Food order retrieved successfully.",
            Data = response
        };
    }
}
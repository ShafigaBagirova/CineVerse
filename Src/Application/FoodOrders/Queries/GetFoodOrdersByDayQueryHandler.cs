using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.FoodOrders.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.FoodOrders.Queries;

public class GetFoodOrdersByDayQueryHandler
    : IRequestHandler<GetFoodOrdersByDayQuery, BaseResponse<List<OrdersByDayResponse>>>
{
    private readonly IFoodOrderRepository _foodOrderRepository;
    private readonly ILogger<GetFoodOrdersByDayQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetFoodOrdersByDayQueryHandler(
        IFoodOrderRepository foodOrderRepository,
        ILogger<GetFoodOrdersByDayQueryHandler> logger,
        ICacheService cacheService)
    {
        _foodOrderRepository = foodOrderRepository;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<List<OrdersByDayResponse>>> Handle(
        GetFoodOrdersByDayQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "GetFoodOrdersByDayQuery started. CinemaId: {CinemaId}, ScreeningId: {ScreeningId}, From: {From}, To: {To}",
            request.Request.CinemaId,
            request.Request.ScreeningId,
            request.Request.From,
            request.Request.To);

        var cacheKey =
            $"foodorders:ordersbyday:" +
            $"cinema:{(request.Request.CinemaId.HasValue ? request.Request.CinemaId.Value.ToString() : "null")}:" +
            $"screening:{(request.Request.ScreeningId.HasValue ? request.Request.ScreeningId.Value.ToString() : "null")}:" +
            $"from:{(request.Request.From.HasValue ? request.Request.From.Value.ToString("yyyyMMddHHmmss") : "null")}:" +
            $"to:{(request.Request.To.HasValue ? request.Request.To.Value.ToString("yyyyMMddHHmmss") : "null")}";

        var cachedData = await _cacheService.GetAsync<List<OrdersByDayResponse>>(cacheKey);

        if (cachedData is not null)
        {
            _logger.LogInformation(
                "Orders by day retrieved from cache. CacheKey: {CacheKey}",
                cacheKey);

            return new BaseResponse<List<OrdersByDayResponse>>
            {
                Success = true,
                Message = "Orders by day retrieved successfully from cache.",
                Data = cachedData
            };
        }

        var result = await _foodOrderRepository.GetOrdersByDayAsync(
            request.Request.CinemaId,
            request.Request.ScreeningId,
            request.Request.From,
            request.Request.To,
            cancellationToken);

        await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

        _logger.LogInformation(
            "GetFoodOrdersByDayQuery completed successfully. Count: {Count}",
            result.Count);

        return new BaseResponse<List<OrdersByDayResponse>>
        {
            Success = true,
            Message = "Orders by day retrieved successfully.",
            Data = result
        };
    }
}
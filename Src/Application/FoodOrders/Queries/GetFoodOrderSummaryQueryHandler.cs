using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.FoodOrders.Dtos;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.FoodOrders.Queries;

public class GetFoodOrderSummaryQueryHandler
    : IRequestHandler<GetFoodOrderSummaryQuery, BaseResponse<FoodOrderSummaryResponse>>
{
    private readonly IFoodOrderRepository _foodOrderRepository;
    private readonly ILogger<GetFoodOrderSummaryQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetFoodOrderSummaryQueryHandler(
        IFoodOrderRepository foodOrderRepository,
        ILogger<GetFoodOrderSummaryQueryHandler> logger,
        ICacheService cacheService)
    {
        _foodOrderRepository = foodOrderRepository;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<FoodOrderSummaryResponse>> Handle(
        GetFoodOrderSummaryQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "GetFoodOrderSummaryQuery started. CinemaId: {CinemaId}, ScreeningId: {ScreeningId}, CreatedFrom: {CreatedFrom}, CreatedTo: {CreatedTo}",
            request.Request.CinemaId,
            request.Request.ScreeningId,
            request.Request.CreatedFrom,
            request.Request.CreatedTo);

        var cacheKey =
            $"foodordersummary:" +
            $"cinema:{(request.Request.CinemaId.HasValue ? request.Request.CinemaId.Value.ToString() : "null")}:" +
            $"screening:{(request.Request.ScreeningId.HasValue ? request.Request.ScreeningId.Value.ToString() : "null")}:" +
            $"from:{(request.Request.CreatedFrom.HasValue ? request.Request.CreatedFrom.Value.ToString("yyyyMMddHHmmss") : "null")}:" +
            $"to:{(request.Request.CreatedTo.HasValue ? request.Request.CreatedTo.Value.ToString("yyyyMMddHHmmss") : "null")}";

        var cachedData = await _cacheService.GetAsync<FoodOrderSummaryResponse>(cacheKey);

        if (cachedData is not null)
        {
            _logger.LogInformation("Food order summary retrieved from cache. CacheKey: {CacheKey}", cacheKey);

            return new BaseResponse<FoodOrderSummaryResponse>
            {
                Success = true,
                Message = "Food order summary retrieved successfully from cache.",
                Data = cachedData
            };
        }

        var query = await _foodOrderRepository.GetQueryableAsync();

        if (request.Request.CinemaId.HasValue)
        {
            query = query.Where(x => x.CinemaId == request.Request.CinemaId.Value);
        }

        if (request.Request.ScreeningId.HasValue)
        {
            query = query.Where(x => x.ScreeningId == request.Request.ScreeningId.Value);
        }

        if (request.Request.CreatedFrom.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= request.Request.CreatedFrom.Value);
        }

        if (request.Request.CreatedTo.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= request.Request.CreatedTo.Value);
        }

        var orders = await _foodOrderRepository.ToListAsync(query, cancellationToken);

        var response = new FoodOrderSummaryResponse
        {
            TotalOrders = orders.Count,
            TotalRevenue = orders
                .Where(x => x.Status == FoodOrderStatus.Confirmed || x.Status == FoodOrderStatus.Delivered)
                .Sum(x => x.TotalAmount),

            PendingOrders = orders.Count(x => x.Status == FoodOrderStatus.Pending),
            ConfirmedOrders = orders.Count(x => x.Status == FoodOrderStatus.Confirmed),
            DeliveredOrders = orders.Count(x => x.Status == FoodOrderStatus.Delivered),
            CancelledOrders = orders.Count(x => x.Status == FoodOrderStatus.Cancelled),
            RefundedOrders = orders.Count(x => x.Status == FoodOrderStatus.Refunded)
        };

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));

        _logger.LogInformation(
            "GetFoodOrderSummaryQuery completed successfully. TotalOrders: {TotalOrders}, TotalRevenue: {TotalRevenue}",
            response.TotalOrders,
            response.TotalRevenue);

        return new BaseResponse<FoodOrderSummaryResponse>
        {
            Success = true,
            Message = "Food order summary retrieved successfully.",
            Data = response
        };
    }
}
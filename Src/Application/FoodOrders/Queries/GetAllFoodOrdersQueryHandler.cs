using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.FoodOrders.Dtos;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.FoodOrders.Queries;

public class GetAllFoodOrdersQueryHandler
    : IRequestHandler<GetAllFoodOrdersQuery, BaseResponse<List<FoodOrderResponse>>>
{
    private readonly IFoodOrderRepository _foodOrderRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetAllFoodOrdersQueryHandler> _logger;
    private readonly ICacheService _cacheService;

    public GetAllFoodOrdersQueryHandler(
        IFoodOrderRepository foodOrderRepository,
        IMapper mapper,
        ILogger<GetAllFoodOrdersQueryHandler> logger,
        ICacheService cacheService)
    {
        _foodOrderRepository = foodOrderRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<List<FoodOrderResponse>>> Handle(
        GetAllFoodOrdersQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "GetAllFoodOrdersQuery started. UserId: {UserId}, TicketId: {TicketId},SeatId:{SeatId},SeatHoldId:{SeatHoldId}," +
            "CinemaId:{CinemaId},ScreeninId:{ScreeningId}, Status: {Status}, CreatedFrom: {CreatedFrom}, CreatedTo: {CreatedTo}, PageNumber: {PageNumber}, PageSize: {PageSize}",
            request.Request.UserId,
            request.Request.SeatHoldId,
            request.Request.SeatId,
            request.Request.ScreeningId,
            request.Request.CinemaId,
            request.Request.Status,
            request.Request.CreatedFrom,
            request.Request.CreatedTo,
            request.Request.PageNumber,
            request.Request.PageSize,
            request.Request.DeliveryType);

        var cacheKey =
            $"foodorders:" +
            $"user:{(request.Request.UserId)}:" +
            $"seathold:{(request.Request.SeatHoldId.HasValue ? request.Request.SeatHoldId.Value.ToString() : "null")}:" +
            $"screening:{(request.Request.ScreeningId.HasValue ? request.Request.ScreeningId.Value.ToString() : "null")}:" +
            $"seat:{(request.Request.SeatId.HasValue ? request.Request.SeatId.Value.ToString() : "null")}:" +
            $"cinema:{(request.Request.CinemaId.HasValue ? request.Request.CinemaId.Value.ToString() : "null")}:" +
            $"status:{(request.Request.Status)}:" +
            $"delivery:{(request.Request.DeliveryType)}:" +
            $"from:{(request.Request.CreatedFrom.HasValue ? request.Request.CreatedFrom.Value.ToString("yyyyMMddHHmmss") : "null")}:" +
            $"to:{(request.Request.CreatedTo.HasValue ? request.Request.CreatedTo.Value.ToString("yyyyMMddHHmmss") : "null")}:" +
            $"page:{request.Request.PageNumber}:" +
            $"size:{request.Request.PageSize}";


        var cachedData = await _cacheService.GetAsync<List<FoodOrderResponse>>(cacheKey);

        if (cachedData is not null)
        {
            _logger.LogInformation("Food orders retrieved from cache. CacheKey: {CacheKey}", cacheKey);

            return new BaseResponse<List<FoodOrderResponse>>
            {
                Success = true,
                Message = "Food orders retrieved successfully from cache.",
                Data = cachedData
            };
        }

        var query = await _foodOrderRepository.GetQueryableAsync();
       
        if (!string.IsNullOrWhiteSpace(request.Request.UserId))
        {
            query = query.Where(x => x.UserId == request.Request.UserId);
        }
        if (request.Request.SeatHoldId.HasValue)
        {
            query = query.Where(x => x.SeatHoldId == request.Request.SeatHoldId.Value);
        }

        if (request.Request.ScreeningId.HasValue)
        {
            query = query.Where(x => x.ScreeningId == request.Request.ScreeningId.Value);
        }

        if (request.Request.SeatId.HasValue)
        {
            query = query.Where(x => x.SeatId == request.Request.SeatId.Value);
        }
        if(request.Request.DeliveryType.HasValue)
        {
            query=query.Where(x=>x.DeliveryType== request.Request.DeliveryType.Value);
        }

        if (request.Request.Status.HasValue)
        {
            query = query.Where(x => x.Status == request.Request.Status.Value);
        }
        if (request.Request.CreatedFrom.HasValue)
        {
            query = query.Where(x => x.CreatedAt >= request.Request.CreatedFrom.Value);
        }

        if (request.Request.CreatedTo.HasValue)
        {
            query = query.Where(x => x.CreatedAt <= request.Request.CreatedTo.Value);
        }

        query = query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((request.Request.PageNumber - 1) * request.Request.PageSize)
            .Take(request.Request.PageSize);

        var orders = await _foodOrderRepository.ToListAsync(query, cancellationToken);

        var response = _mapper.Map<List<FoodOrderResponse>>(orders);

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));

        _logger.LogInformation(
            "GetAllFoodOrdersQuery completed successfully. ReturnedCount: {Count}",
            response.Count);

        return new BaseResponse<List<FoodOrderResponse>>
        {
            Success = true,
            Message = "Food orders retrieved successfully.",
            Data = response
        };
    }
}
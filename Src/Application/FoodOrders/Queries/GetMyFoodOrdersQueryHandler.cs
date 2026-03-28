using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.FoodOrders.Dtos;
using AutoMapper;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.FoodOrders.Queries;

public class GetMyFoodOrdersQueryHandler
    : IRequestHandler<GetMyFoodOrdersQuery, BaseResponse<List<FoodOrderResponse>>>
{
    private readonly IFoodOrderRepository _foodOrderRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<GetMyFoodOrdersQueryHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly ICurrentUserService _currentUserService;

    public GetMyFoodOrdersQueryHandler(
        IFoodOrderRepository foodOrderRepository,
        IMapper mapper,
        ILogger<GetMyFoodOrdersQueryHandler> logger,
        ICacheService cacheService,
        ICurrentUserService currentUserService)
    {
        _foodOrderRepository = foodOrderRepository;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
        _currentUserService = currentUserService;
    }

    public async Task<BaseResponse<List<FoodOrderResponse>>> Handle(
        GetMyFoodOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("GetMyFoodOrdersQuery failed because current user id is null.");

            return new BaseResponse<List<FoodOrderResponse>>
            {
                Success = false,
                Message = "User information could not be resolved."
            };
        }

        _logger.LogInformation(
            "GetMyFoodOrdersQuery started. UserId: {UserId}, SeatHoldId: {SeatHoldId}, ScreeningId: {ScreeningId}, SeatId: {SeatId}, CinemaId: {CinemaId}, Status: {Status}, DeliveryType: {DeliveryType}, PageNumber: {PageNumber}, PageSize: {PageSize}",
            userId,
            request.Request.SeatHoldId,
            request.Request.ScreeningId,
            request.Request.SeatId,
            request.Request.CinemaId,
            request.Request.Status,
            request.Request.DeliveryType,
            request.Request.PageNumber,
            request.Request.PageSize);

        static string Normalize(string? value)
            => string.IsNullOrWhiteSpace(value) ? "null" : value.Trim().ToLower();

        var cacheKey =
            $"myfoodorders:" +
            $"user:{Normalize(userId)}:" +
            $"seathold:{(request.Request.SeatHoldId.HasValue ? request.Request.SeatHoldId.Value.ToString() : "null")}:" +
            $"screening:{(request.Request.ScreeningId.HasValue ? request.Request.ScreeningId.Value.ToString() : "null")}:" +
            $"seat:{(request.Request.SeatId.HasValue ? request.Request.SeatId.Value.ToString() : "null")}:" +
            $"cinema:{(request.Request.CinemaId.HasValue ? request.Request.CinemaId.Value.ToString() : "null")}:" +
           $"status:{(request.Request.Status.HasValue ? request.Request.Status.Value.ToString().ToLower() : "null")}:" +
           $"delivery:{(request.Request.DeliveryType.HasValue ? request.Request.DeliveryType.Value.ToString().ToLower() : "null")}:" +
            $"from:{(request.Request.CreatedFrom.HasValue ? request.Request.CreatedFrom.Value.ToString("yyyyMMddHHmmss") : "null")}:" +
            $"to:{(request.Request.CreatedTo.HasValue ? request.Request.CreatedTo.Value.ToString("yyyyMMddHHmmss") : "null")}:" +
            $"page:{request.Request.PageNumber}:" +
            $"size:{request.Request.PageSize}";

        var cachedData = await _cacheService.GetAsync<List<FoodOrderResponse>>(cacheKey);

        if (cachedData is not null)
        {
            _logger.LogInformation("My food orders retrieved from cache. CacheKey: {CacheKey}", cacheKey);

            return new BaseResponse<List<FoodOrderResponse>>
            {
                Success = true,
                Message = "My food orders retrieved successfully from cache.",
                Data = cachedData
            };
        }

        var query = await _foodOrderRepository.GetQueryableAsync();

        query = query.Where(x => x.UserId == userId);

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

        if (request.Request.CinemaId.HasValue)
        {
            query = query.Where(x => x.CinemaId == request.Request.CinemaId.Value);
        }
        if (request.Request.Status.HasValue)
        {
            query=query.Where(x=>x.Status == request.Request.Status.Value);
        }
        if (request.Request.DeliveryType.HasValue)
        {
            query=query.Where(x=>x.DeliveryType == request.Request.DeliveryType.Value);
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
            "GetMyFoodOrdersQuery completed successfully. UserId: {UserId}, ReturnedCount: {Count}",
            userId,
            response.Count);

        return new BaseResponse<List<FoodOrderResponse>>
        {
            Success = true,
            Message = "My food orders retrieved successfully.",
            Data = response
        };
    }
}
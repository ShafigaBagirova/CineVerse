using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.FoodOrders.Commands;

public sealed class CancelFoodOrderCommandHandler
    : IRequestHandler<CancelFoodOrderCommand, BaseResponse>
{
    private readonly IFoodOrderRepository _foodOrderRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CancelFoodOrderCommandHandler> _logger;
    private readonly ICacheService _cacheService;

    public CancelFoodOrderCommandHandler(
        IFoodOrderRepository foodOrderRepository,
        ICurrentUserService currentUserService,
        ILogger<CancelFoodOrderCommandHandler> logger,
        ICacheService cacheService)
    {
        _foodOrderRepository = foodOrderRepository;
        _currentUserService = currentUserService;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse> Handle(
        CancelFoodOrderCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
            return BaseResponse.Fail("Authenticated user not found.");

        _logger.LogInformation(
            "CancelFoodOrderCommand started. FoodOrderId: {FoodOrderId}, UserId: {UserId}",
            request.Id,
            userId);

        var foodOrder = await _foodOrderRepository.GetByIdAsync(request.Id, cancellationToken);

        if (foodOrder is null)
            return BaseResponse.Fail("Food order not found.");

        if (foodOrder.UserId != userId)
            return BaseResponse.Fail("You are not allowed to cancel this food order.");

        if (foodOrder.Status != FoodOrderStatus.Pending)
            return BaseResponse.Fail("Only pending food orders can be cancelled.");

        foodOrder.Status = FoodOrderStatus.Cancelled;
        foodOrder.CancelledAtUtc = DateTime.UtcNow;

        await _foodOrderRepository.UpdateAsync(foodOrder, cancellationToken);
        await _foodOrderRepository.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync(FoodOrderCacheKey.GetById(foodOrder.Id), cancellationToken);
        await _cacheService.RemoveByPrefixAsync(FoodOrderCacheKey.AllPrefix);
        await _cacheService.RemoveByPrefixAsync(FoodOrderCacheKey.MyOrdersPrefix);
        await _cacheService.RemoveAsync(FoodOrderCacheKey.SummaryPrefix, cancellationToken);
        _logger.LogInformation(
            "CancelFoodOrderCommand completed successfully. FoodOrderId: {FoodOrderId}",
            foodOrder.Id);

        return BaseResponse.Ok("Food order cancelled successfully.");
    }
}
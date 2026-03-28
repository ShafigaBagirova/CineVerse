using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.FoodOrders.Commands;

public sealed class UpdateFoodOrderDraftCommandHandler
    : IRequestHandler<UpdateFoodOrderDraftCommand, BaseResponse>
{
    private readonly IFoodOrderRepository _foodOrderRepository;
    private readonly IFoodOrderItemRepository _foodOrderItemRepository;
    private readonly IFoodItemRepository _foodItemRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateFoodOrderDraftCommandHandler> _logger;
    private readonly ICacheService _cacheService;
    public UpdateFoodOrderDraftCommandHandler(
        IFoodOrderRepository foodOrderRepository,
        IFoodOrderItemRepository foodOrderItemRepository,
        IFoodItemRepository foodItemRepository,
        ICurrentUserService currentUserService,
        ILogger<UpdateFoodOrderDraftCommandHandler> logger,
        ICacheService cacheService)
    {
        _foodOrderRepository = foodOrderRepository;
        _foodOrderItemRepository = foodOrderItemRepository;
        _foodItemRepository = foodItemRepository;
        _currentUserService = currentUserService;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse> Handle(
        UpdateFoodOrderDraftCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
            return BaseResponse.Fail("Authenticated user not found.");

        _logger.LogInformation(
            "UpdateFoodOrderDraftCommand started. FoodOrderId: {FoodOrderId}, UserId: {UserId}",
            request.Id,
            userId);

        var foodOrder = await _foodOrderRepository.GetByIdWithItemsAsync(
            request.Id,
            cancellationToken);

        if (foodOrder is null)
            return BaseResponse.Fail("Food order not found.");

        if (foodOrder.UserId != userId)
            return BaseResponse.Fail("You are not allowed to update this food order.");

        if (foodOrder.Status != FoodOrderStatus.Pending)
            return BaseResponse.Fail("Only pending food orders can be updated.");

        var foodItemIds = request.Request.Items
            .Select(x => x.FoodItemId)
            .Distinct()
            .ToList();

        var foodItems = await _foodItemRepository.GetByIdsAsync(foodItemIds, cancellationToken);

        if (foodItems.Count != foodItemIds.Count)
            return BaseResponse.Fail("One or more food items are invalid or unavailable.");

        foodOrder.FoodOrderItems.Clear();

        decimal totalAmount = 0;

        foreach (var item in request.Request.Items)
        {
            var foodItem = foodItems.First(x => x.Id == item.FoodItemId);
            var totalPrice = foodItem.Price * item.Quantity;

            foodOrder.FoodOrderItems.Add(new FoodOrderItem
            {
                FoodOrderId = foodOrder.Id,
                FoodItemId = foodItem.Id,
                Quantity = item.Quantity,
                UnitPrice = foodItem.Price,
                TotalPrice = totalPrice
            });

            totalAmount += totalPrice;
        }

        foodOrder.TotalAmount = totalAmount;
        foodOrder.DeliveryType = request.Request.DeliveryType;
        foodOrder.Note = request.Request.Note;

        await _foodOrderRepository.UpdateAsync(foodOrder, cancellationToken);
        await _foodOrderRepository.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveAsync(FoodOrderCacheKey.GetById(foodOrder.Id), cancellationToken);
        await _cacheService.RemoveByPrefixAsync(FoodOrderCacheKey.AllPrefix);
        await _cacheService.RemoveByPrefixAsync(FoodOrderCacheKey.MyOrdersPrefix);
        await _cacheService.RemoveByPrefixAsync(FoodOrderCacheKey.SummaryPrefix);

        _logger.LogInformation(
            "UpdateFoodOrderDraftCommand completed successfully. FoodOrderId: {FoodOrderId}, TotalAmount: {TotalAmount}",
            foodOrder.Id,
            foodOrder.TotalAmount);

        return BaseResponse.Ok("Food order draft updated successfully.");
    }
}
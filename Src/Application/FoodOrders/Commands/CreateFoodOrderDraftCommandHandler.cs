using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.FoodOrders.Commands;

public sealed class CreateFoodOrderDraftCommandHandler
    : IRequestHandler<CreateFoodOrderDraftCommand, BaseResponse>
{
    private readonly IFoodOrderRepository _foodOrderRepository;
    private readonly IFoodOrderItemRepository _foodOrderItemRepository;
    private readonly IFoodItemRepository _foodItemRepository;
    private readonly ISeatHoldRepository _seatHoldRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateFoodOrderDraftCommandHandler> _logger;
    private readonly ICacheService _cacheService;

    public CreateFoodOrderDraftCommandHandler(
        IFoodOrderRepository foodOrderRepository,
        IFoodOrderItemRepository foodOrderItemRepository,
        IFoodItemRepository foodItemRepository,
        ISeatHoldRepository seatHoldRepository,
        ICurrentUserService currentUserService,
        ILogger<CreateFoodOrderDraftCommandHandler> logger,
        ICacheService cacheService)
    {
        _foodOrderRepository = foodOrderRepository;
        _foodOrderItemRepository = foodOrderItemRepository;
        _foodItemRepository = foodItemRepository;
        _seatHoldRepository = seatHoldRepository;
        _currentUserService = currentUserService;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse> Handle(
        CreateFoodOrderDraftCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
            return BaseResponse.Fail("Authenticated user not found.");

        _logger.LogInformation(
            "CreateFoodOrderDraftCommand started. UserId: {UserId}, SeatHoldId: {SeatHoldId}",
            userId,
            request.Request.SeatHoldId);

        var seatHold = await _seatHoldRepository.GetByIdAsync(
            request.Request.SeatHoldId,
            cancellationToken);

        if (seatHold is null)
            return BaseResponse.Fail("Seat hold not found.");

        if (seatHold.UserId != userId)
            return BaseResponse.Fail("You are not allowed to create a food order for this seat hold.");

        if (seatHold.Status != SeatHoldStatus.Active)
            return BaseResponse.Fail("Food order can only be created for an active seat hold.");

        var existingPendingOrder = await _foodOrderRepository.GetPendingBySeatHoldIdAsync(
            request.Request.SeatHoldId,
            cancellationToken);

        if (existingPendingOrder is not null)
            return BaseResponse.Fail("A pending food order already exists for this seat hold.");

        var foodItemIds = request.Request.Items
            .Select(x => x.FoodItemId)
            .Distinct()
            .ToList();

        var foodItems = await _foodItemRepository.GetByIdsAsync(foodItemIds, cancellationToken);

        if (foodItems.Count != foodItemIds.Count)
            return BaseResponse.Fail("One or more food items are invalid or unavailable.");

        var foodOrder = new FoodOrder
        {
            UserId = userId,
            SeatHoldId = seatHold.Id,
            ScreeningId = seatHold.ScreeningId,
            SeatId = seatHold.SeatId,
            DeliveryType = request.Request.DeliveryType,
            Status = FoodOrderStatus.Pending,
            Note = request.Request.Note,
            CreatedAt = DateTime.UtcNow
        };

        await _foodOrderRepository.AddAsync(foodOrder, cancellationToken);
        await _foodOrderRepository.SaveChangesAsync(cancellationToken);
        await _cacheService.RemoveByPrefixAsync(FoodOrderCacheKey.AllPrefix);
        await _cacheService.RemoveByPrefixAsync(FoodOrderCacheKey.MyOrdersPrefix);
        await _cacheService.RemoveByPrefixAsync(FoodOrderCacheKey.SummaryPrefix);

        var orderItems = new List<FoodOrderItem>();
        decimal totalAmount = 0;

        foreach (var item in request.Request.Items)
        {
            var foodItem = foodItems.First(x => x.Id == item.FoodItemId);
            var totalPrice = foodItem.Price * item.Quantity;

            orderItems.Add(new FoodOrderItem
            {
                FoodOrderId = foodOrder.Id,
                FoodItemId = foodItem.Id,
                Quantity = item.Quantity,
                UnitPrice = foodItem.Price,
                TotalPrice = totalPrice
            });

            totalAmount += totalPrice;
        }

        await _foodOrderItemRepository.AddRangeAsync(orderItems, cancellationToken);

        foodOrder.TotalAmount = totalAmount;

        await _foodOrderRepository.UpdateAsync(foodOrder, cancellationToken);
        await _foodOrderRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "CreateFoodOrderDraftCommand completed successfully. FoodOrderId: {FoodOrderId}, TotalAmount: {TotalAmount}",
            foodOrder.Id,
            foodOrder.TotalAmount);

        return BaseResponse.Ok("Food order draft created successfully.");
    }
}
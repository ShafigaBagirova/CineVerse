using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.FoodOrders.Dtos;
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
    private readonly IScreeningRepository _screeningRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreateFoodOrderDraftCommandHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IMediator _mediator;

    public CreateFoodOrderDraftCommandHandler(
        IFoodOrderRepository foodOrderRepository,
        IFoodOrderItemRepository foodOrderItemRepository,
        IFoodItemRepository foodItemRepository,
        ISeatHoldRepository seatHoldRepository,
        IScreeningRepository screeningRepository,
        ICurrentUserService currentUserService,
        ILogger<CreateFoodOrderDraftCommandHandler> logger,
        ICacheService cacheService,
        IMediator mediator)
    {
        _foodOrderRepository = foodOrderRepository;
        _foodOrderItemRepository = foodOrderItemRepository;
        _foodItemRepository = foodItemRepository;
        _seatHoldRepository = seatHoldRepository;
        _screeningRepository = screeningRepository;
        _currentUserService = currentUserService;
        _logger = logger;
        _cacheService = cacheService;
        _mediator = mediator;
    }

    public async Task<BaseResponse> Handle(
        CreateFoodOrderDraftCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("CreateFoodOrderDraft rejected: authenticated user id is missing.");
            return BaseResponse.Fail("Authenticated user not found.");
        }

        _logger.LogInformation(
            "CreateFoodOrderDraftCommand started. UserId: {UserId}, SeatHoldId: {SeatHoldId}",
            userId,
            request.Request.SeatHoldId);

        var seatHold = await _seatHoldRepository.GetByIdAsync(
            request.Request.SeatHoldId,
            cancellationToken);

        if (seatHold is null)
        {
            _logger.LogWarning(
                "CreateFoodOrderDraft rejected: seat hold not found. SeatHoldId: {SeatHoldId}",
                request.Request.SeatHoldId);
            return BaseResponse.Fail("Seat hold not found.");
        }

        if (!string.Equals(seatHold.UserId, userId, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning(
                "CreateFoodOrderDraft rejected: seat hold belongs to another user. SeatHoldId: {SeatHoldId}",
                request.Request.SeatHoldId);
            return BaseResponse.Fail("You are not allowed to create a food order for this seat hold.");
        }

        if (seatHold.Status != SeatHoldStatus.Active)
        {
            _logger.LogWarning(
                "CreateFoodOrderDraft rejected: seat hold is not active. SeatHoldId: {SeatHoldId}, Status: {Status}",
                request.Request.SeatHoldId,
                seatHold.Status);
            return BaseResponse.Fail("Food order can only be created for an active seat hold.");
        }

        var cinemaId = await _screeningRepository.GetCinemaIdForScreeningAsync(
            seatHold.ScreeningId,
            cancellationToken);

        if (cinemaId is null or <= 0)
        {
            _logger.LogWarning(
                "CreateFoodOrderDraft rejected: could not resolve cinema for screening. ScreeningId: {ScreeningId}, SeatHoldId: {SeatHoldId}",
                seatHold.ScreeningId,
                seatHold.Id);
            return BaseResponse.Fail("Could not resolve cinema for this screening.");
        }

        var existingPendingOrder = await _foodOrderRepository.GetPendingBySeatHoldIdAsync(
            request.Request.SeatHoldId,
            cancellationToken);

        if (existingPendingOrder is not null)
        {
            _logger.LogInformation(
                "CreateFoodOrderDraft: pending order already exists for seat hold; updating draft. FoodOrderId: {FoodOrderId}, SeatHoldId: {SeatHoldId}",
                existingPendingOrder.Id,
                request.Request.SeatHoldId);

            return await _mediator.Send(
                new UpdateFoodOrderDraftCommand(
                    existingPendingOrder.Id,
                    new UpdateFoodOrderDraftRequest
                    {
                        Items = request.Request.Items,
                        DeliveryType = request.Request.DeliveryType,
                        Note = request.Request.Note
                    }),
                cancellationToken);
        }

        var foodItemIds = request.Request.Items
            .Select(x => x.FoodItemId)
            .Distinct()
            .ToList();

        var foodItems = await _foodItemRepository.GetByIdsAsync(foodItemIds, cancellationToken);

        if (foodItems.Count != foodItemIds.Count)
        {
            var foundIds = foodItems.Select(x => x.Id).ToHashSet();
            var missing = foodItemIds.Where(id => !foundIds.Contains(id)).ToList();
            _logger.LogWarning(
                "CreateFoodOrderDraft rejected: one or more food items invalid or unavailable (inactive/unavailable or unknown id). RequestedDistinctIds: {Requested}, MissingOrUnavailable: {Missing}",
                foodItemIds,
                missing);
            return BaseResponse.Fail("One or more food items are invalid or unavailable.");
        }

        var foodOrder = new FoodOrder
        {
            UserId = userId,
            SeatHoldId = seatHold.Id,
            ScreeningId = seatHold.ScreeningId,
            SeatId = seatHold.SeatId,
            CinemaId = cinemaId.Value,
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
using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Payments.Dtos;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Payments.Commands;

public sealed class RetryPaymentCommandHandler
    : IRequestHandler<RetryPaymentCommand, BaseResponse<RetryPaymentResponse>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ISeatHoldRepository _seatHoldRepository;
    private readonly IStripeService _stripeService;
    private readonly ILogger<RetryPaymentCommandHandler> _logger;
    private readonly ICacheService _cacheService;
    private readonly IScreeningRepository _screeningRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFoodOrderRepository _foodOrderRepository;

    public RetryPaymentCommandHandler(
        IPaymentRepository paymentRepository,
        ISeatHoldRepository seatHoldRepository,
        IStripeService stripeService,
        ILogger<RetryPaymentCommandHandler> logger,
        ICacheService cacheService,
        IScreeningRepository screeningRepository,
        ICurrentUserService currentUserService,
        IFoodOrderRepository foodOrderRepository)
    {
        _paymentRepository = paymentRepository;
        _seatHoldRepository = seatHoldRepository;
        _stripeService = stripeService;
        _logger = logger;
        _cacheService = cacheService;
        _screeningRepository = screeningRepository;
        _currentUserService = currentUserService;
        _foodOrderRepository = foodOrderRepository;
    }

    public async Task<BaseResponse<RetryPaymentResponse>> Handle(
        RetryPaymentCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning(
                "Retry payment failed. Authenticated user not found. SeatHoldId: {SeatHoldId}",
                request.SeatHoldId);

            return BaseResponse<RetryPaymentResponse>.Fail("Authenticated user not found.");
        }

        _logger.LogInformation(
            "RetryPaymentCommand started. SeatHoldId: {SeatHoldId}, UserId: {UserId}",
            request.SeatHoldId,
            userId);

        var seatHold = await _seatHoldRepository.GetByIdAsync(request.SeatHoldId, cancellationToken);

        if (seatHold is null)
        {
            _logger.LogWarning(
                "Retry payment failed. SeatHold not found. SeatHoldId: {SeatHoldId}",
                request.SeatHoldId);

            return BaseResponse<RetryPaymentResponse>.Fail("Seat hold not found.");
        }

        if (seatHold.UserId != userId)
        {
            _logger.LogWarning(
                "Retry payment failed. Unauthorized user. SeatHoldId: {SeatHoldId}, OwnerUserId: {OwnerUserId}, CurrentUserId: {CurrentUserId}",
                seatHold.Id,
                seatHold.UserId,
                userId);

            return BaseResponse<RetryPaymentResponse>.Fail(
                "You are not allowed to retry payment for this seat hold.");
        }

        var succeededPaymentExists = await _paymentRepository.ExistsSucceededPaymentBySeatHoldIdAsync(
            seatHold.Id,
            cancellationToken);

        if (succeededPaymentExists)
        {
            _logger.LogWarning(
                "Retry payment failed. Successful payment already exists. SeatHoldId: {SeatHoldId}",
                seatHold.Id);

            return BaseResponse<RetryPaymentResponse>.Fail(
                "Payment has already been completed for this seat hold.");
        }

        if (seatHold.Status == SeatHoldStatus.Expired)
        {
            _logger.LogWarning(
                "Retry payment failed. SeatHold expired. SeatHoldId: {SeatHoldId}",
                request.SeatHoldId);

            return BaseResponse<RetryPaymentResponse>.Fail("Seat hold has expired.");
        }

        if (seatHold.Status == SeatHoldStatus.Released)
        {
            _logger.LogWarning(
                "Retry payment failed. SeatHold released. SeatHoldId: {SeatHoldId}",
                request.SeatHoldId);

            return BaseResponse<RetryPaymentResponse>.Fail("Seat hold has been released.");
        }

        if (seatHold.Status == SeatHoldStatus.Purchased)
        {
            _logger.LogWarning(
                "Retry payment failed. Seat already purchased. SeatHoldId: {SeatHoldId}",
                request.SeatHoldId);

            return BaseResponse<RetryPaymentResponse>.Fail(
                "Ticket has already been purchased for this seat hold.");
        }

        if (seatHold.Status != SeatHoldStatus.Active)
        {
            _logger.LogWarning(
                "Retry payment failed. Invalid SeatHold status. SeatHoldId: {SeatHoldId}, Status: {Status}",
                request.SeatHoldId,
                seatHold.Status);

            return BaseResponse<RetryPaymentResponse>.Fail("Seat hold is not active.");
        }

        var pendingPaymentExists = await _paymentRepository.ExistsPendingPaymentBySeatHoldIdAsync(
            seatHold.Id,
            cancellationToken);

        if (pendingPaymentExists)
        {
            _logger.LogWarning(
                "Retry payment failed. Pending payment already exists. SeatHoldId: {SeatHoldId}",
                seatHold.Id);

            return BaseResponse<RetryPaymentResponse>.Fail(
                "A pending payment already exists for this seat hold.");
        }

        var screening = await _screeningRepository.GetByIdAsync(seatHold.ScreeningId, cancellationToken);

        if (screening is null)
        {
            _logger.LogWarning(
                "Retry payment failed. Screening not found. ScreeningId: {ScreeningId}, SeatHoldId: {SeatHoldId}",
                seatHold.ScreeningId,
                seatHold.Id);

            return BaseResponse<RetryPaymentResponse>.Fail("Screening not found.");
        }
        var foodOrder = await _foodOrderRepository.GetActiveBySeatHoldIdAsync(seatHold.Id,cancellationToken);
        if (screening.Price <= 0)
        {
            _logger.LogWarning(
                "Retry payment failed. Invalid screening price. ScreeningId: {ScreeningId}, Price: {Price}",
                screening.Id,
                screening.Price);

            return BaseResponse<RetryPaymentResponse>.Fail("Invalid payment amount.");
        }


        var ticketAmount = screening.Price;
        var foodAmount = foodOrder?.TotalAmount ?? 0m;
        var totalAmount = ticketAmount + foodAmount;

        if (ticketAmount <= 0 || totalAmount <= 0)
        {
            _logger.LogWarning(
                "Retry payment failed. Invalid payment amount. ScreeningId: {ScreeningId}, TicketAmount: {TicketAmount}, FoodAmount: {FoodAmount}, TotalAmount: {TotalAmount}",
                screening.Id,
                ticketAmount,
                foodAmount,
                totalAmount);

            return BaseResponse<RetryPaymentResponse>.Fail("Invalid payment amount.");
        }

        var idempotencyKey = $"retry-payment-{seatHold.Id}-{Guid.NewGuid()}";

        var stripeResult = await _stripeService.CreatePaymentIntentAsync(
            totalAmount,
            "azn",
            idempotencyKey,
            cancellationToken);

        var payment = new Payment
        {
            SeatHoldId = seatHold.Id,
            UserId = seatHold.UserId,
            TicketAmount = ticketAmount,
            FoodAmount = foodAmount,
            TotalAmount = totalAmount,
            Currency = PaymentCurrency.Azn,
            Provider = PaymentProvider.Stripe,
            ProviderPaymentIntentId = stripeResult.PaymentIntentId,
            ClientSecret = stripeResult.ClientSecret,
            Status = PaymentStatus.Pending,
            PaidAtUtc = null
        };

        try
        {
            await _paymentRepository.AddAsync(payment, cancellationToken);
            await _paymentRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(
                ex,
                "Retry payment failed. Duplicate pending payment prevented by database. SeatHoldId: {SeatHoldId}",
                seatHold.Id);

            return BaseResponse<RetryPaymentResponse>.Fail(
                "A payment is already being processed for this seat hold.");
        }

        await _cacheService.RemoveAsync($"payment-status-seatHold:{seatHold.Id}");

        _logger.LogInformation(
            "Retry payment created successfully. PaymentId: {PaymentId}, SeatHoldId: {SeatHoldId}, UserId: {UserId}, ProviderPaymentIntentId: {ProviderPaymentIntentId}",
            payment.Id,
            payment.SeatHoldId,
            payment.UserId,
            payment.ProviderPaymentIntentId);

        var response = new RetryPaymentResponse
        {
            PaymentId = payment.Id,
            SeatHoldId = payment.SeatHoldId,
            Provider = payment.Provider,
            ProviderPaymentIntentId = payment.ProviderPaymentIntentId!,
            ClientSecret = stripeResult.ClientSecret,
            TicketAmount = payment.TicketAmount,
            FoodAmount = payment.FoodAmount,
            TotalAmount = payment.TotalAmount,
            Currency = payment.Currency.ToString(),
            Status = payment.Status
        };

        return BaseResponse<RetryPaymentResponse>.Ok(
            response,
            "Payment retry created successfully.");
    }
}
using Application.Common.Helpers;
using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Payments.Dtos;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Payments.Commands;

public sealed class CreatePaymentIntentCommandHandler
    : IRequestHandler<CreatePaymentIntentCommand, BaseResponse<CreatePaymentIntentResponse>>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ISeatHoldRepository _seatHoldRepository;
    private readonly IScreeningRepository _screeningRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStripeService _stripeService;
    private readonly IMapper _mapper;
    private readonly ILogger<CreatePaymentIntentCommandHandler> _logger;
    private readonly ICacheService _cacheService;

    public CreatePaymentIntentCommandHandler(
        IPaymentRepository paymentRepository,
        ISeatHoldRepository seatHoldRepository,
        IScreeningRepository screeningRepository,
        ICurrentUserService currentUserService,
        IStripeService stripeService,
        IMapper mapper,
        ILogger<CreatePaymentIntentCommandHandler> logger,
        ICacheService cacheService)
    {
        _paymentRepository = paymentRepository;
        _seatHoldRepository = seatHoldRepository;
        _screeningRepository = screeningRepository;
        _currentUserService = currentUserService;
        _stripeService = stripeService;
        _mapper = mapper;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<CreatePaymentIntentResponse>> Handle(
        CreatePaymentIntentCommand request,
        CancellationToken cancellationToken)
    {
        var dto = request.Request;
        var userId = _currentUserService.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("CreatePaymentIntentCommand failed. Authenticated user not found.");

            return BaseResponse<CreatePaymentIntentResponse>.Fail("Authenticated user not found.");
        }

        _logger.LogInformation(
            "CreatePaymentIntentCommand started. SeatHoldId: {SeatHoldId}, UserId: {UserId}",
            dto.SeatHoldId,
            userId);

        var seatHold = await _seatHoldRepository.GetByIdAsync(dto.SeatHoldId, cancellationToken);
        if (seatHold is null)
        {
            _logger.LogWarning(
                "CreatePaymentIntentCommand failed. Seat hold not found. SeatHoldId: {SeatHoldId}",
                dto.SeatHoldId);

            return BaseResponse<CreatePaymentIntentResponse>.Fail("Seat hold not found.");
        }

        if (seatHold.UserId != userId)
        {
            _logger.LogWarning(
                "CreatePaymentIntentCommand failed. User does not own this seat hold. SeatHoldId: {SeatHoldId}, OwnerUserId: {OwnerUserId}, CurrentUserId: {CurrentUserId}",
                seatHold.Id,
                seatHold.UserId,
                userId);

            return BaseResponse<CreatePaymentIntentResponse>.Fail("You are not allowed to pay for this seat hold.");
        }

        if (seatHold.Status != SeatHoldStatus.Active)
        {
            _logger.LogWarning(
                "CreatePaymentIntentCommand failed. Seat hold is not active. SeatHoldId: {SeatHoldId}, Status: {Status}",
                seatHold.Id,
                seatHold.Status);

            return BaseResponse<CreatePaymentIntentResponse>.Fail("Only active seat holds can be paid.");
        }

        if (seatHold.ExpiresAtUtc <= DateTime.UtcNow)
        {
            seatHold.Status = SeatHoldStatus.Expired;

            await _seatHoldRepository.UpdateAsync(seatHold, cancellationToken);
            await _seatHoldRepository.SaveChangesAsync(cancellationToken);

            _logger.LogWarning(
                "CreatePaymentIntentCommand failed. Seat hold expired during payment creation. SeatHoldId: {SeatHoldId}",
                seatHold.Id);

            return BaseResponse<CreatePaymentIntentResponse>.Fail("Seat hold has expired.");
        }

        var succeededPaymentExists = await _paymentRepository.ExistsSucceededPaymentBySeatHoldIdAsync(
            seatHold.Id,
            cancellationToken);

        if (succeededPaymentExists)
        {
            _logger.LogWarning(
                "CreatePaymentIntentCommand failed. Successful payment already exists. SeatHoldId: {SeatHoldId}",
                seatHold.Id);

            return BaseResponse<CreatePaymentIntentResponse>.Fail(
                "Payment has already been completed for this seat hold.");
        }

        var pendingPaymentExists = await _paymentRepository.ExistsPendingPaymentBySeatHoldIdAsync(
            seatHold.Id,
            cancellationToken);

        if (pendingPaymentExists)
        {
            _logger.LogWarning(
                "CreatePaymentIntentCommand failed. Pending payment already exists. SeatHoldId: {SeatHoldId}",
                seatHold.Id);

            return BaseResponse<CreatePaymentIntentResponse>.Fail(
                "A pending payment already exists for this seat hold.");
        }

        var screening = await _screeningRepository.GetByIdAsync(seatHold.ScreeningId, cancellationToken);
        if (screening is null)
        {
            _logger.LogWarning(
                "CreatePaymentIntentCommand failed. Screening not found for seat hold. SeatHoldId: {SeatHoldId}, ScreeningId: {ScreeningId}",
                seatHold.Id,
                seatHold.ScreeningId);

            return BaseResponse<CreatePaymentIntentResponse>.Fail("Screening not found.");
        }

        var amount = screening.Price;

        if (amount <= 0)
        {
            _logger.LogWarning(
                "CreatePaymentIntentCommand failed. Invalid screening price. ScreeningId: {ScreeningId}, Amount: {Amount}",
                screening.Id,
                amount);

            return BaseResponse<CreatePaymentIntentResponse>.Fail("Invalid payment amount.");
        }

        var idempotencyKey = $"create-payment-{seatHold.Id}-{Guid.NewGuid()}";

        var stripeResult = await _stripeService.CreatePaymentIntentAsync(amount,"azn", idempotencyKey, cancellationToken);

        var payment = new Payment
        {
            SeatHoldId = seatHold.Id,
            UserId = userId,
            Amount = amount,
            Currency = PaymentCurrency.Azn,
            Status = PaymentStatus.Pending,
            Provider = PaymentProvider.Stripe,
            ProviderPaymentIntentId = stripeResult.PaymentIntentId,
            ClientSecret = stripeResult.ClientSecret,
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
                "CreatePaymentIntentCommand failed. Duplicate pending payment prevented by database. SeatHoldId: {SeatHoldId}",
                seatHold.Id);

            return BaseResponse<CreatePaymentIntentResponse>.Fail(
                "A payment is already being processed for this seat hold.");
        }

        await _cacheService.RemoveAsync(
            $"{PaymentCacheKeys.GetPaymentsBySeatHoldPrefix}{seatHold.Id}",
            cancellationToken);

        _logger.LogInformation(
            "Payment intent created successfully. PaymentId: {PaymentId}, SeatHoldId: {SeatHoldId}, UserId: {UserId}, ProviderPaymentIntentId: {ProviderPaymentIntentId}, Amount: {Amount}, Currency: {Currency}",
            payment.Id,
            payment.SeatHoldId,
            payment.UserId,
            payment.ProviderPaymentIntentId,
            payment.Amount,
            payment.Currency);

        var response = _mapper.Map<CreatePaymentIntentResponse>(payment);

        return BaseResponse<CreatePaymentIntentResponse>.Ok(
            response,
            "Payment intent created successfully.");
    }
}
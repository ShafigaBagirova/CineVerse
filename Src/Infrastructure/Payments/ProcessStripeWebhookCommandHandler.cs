using Application.Common.Interfaces;
using Application.Common.Options;
using Application.Common.Responses;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;


namespace Infrastructure.Payments;

public sealed class ProcessStripeWebhookCommandHandler
    : IRequestHandler<ProcessStripeWebhookCommand, BaseResponse>
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ISeatHoldRepository _seatHoldRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IProcessedWebhookEventRepository _processedWebhookEventRepository;
    private readonly ILogger<ProcessStripeWebhookCommandHandler> _logger;
    private readonly StripeSettings _stripeSettings;
    private readonly ICacheService _cacheService;

    public ProcessStripeWebhookCommandHandler(
        IPaymentRepository paymentRepository,
        ISeatHoldRepository seatHoldRepository,
        ITicketRepository ticketRepository,
        IProcessedWebhookEventRepository processedWebhookEventRepository,
        ILogger<ProcessStripeWebhookCommandHandler> logger,
        IOptions<StripeSettings> stripeSettings,
        ICacheService cacheService)
    {
        _paymentRepository = paymentRepository;
        _seatHoldRepository = seatHoldRepository;
        _ticketRepository = ticketRepository;
        _processedWebhookEventRepository = processedWebhookEventRepository;
        _logger = logger;
        _stripeSettings = stripeSettings.Value;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse> Handle(ProcessStripeWebhookCommand request, CancellationToken cancellationToken)
    {
        Event stripeEvent;

        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                request.Json,
                request.Signature,
                _stripeSettings.WebhookSecret);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Stripe webhook signature verification failed.");
            return BaseResponse.Fail("Invalid webhook signature.");
        }

        _logger.LogInformation(
            "Stripe webhook received. EventType: {EventType}, EventId: {EventId}",
            stripeEvent.Type,
            stripeEvent.Id);

        var alreadyProcessed = await _processedWebhookEventRepository.ExistsAsync(
            stripeEvent.Id,
            cancellationToken);

        if (alreadyProcessed)
        {
            _logger.LogInformation(
                "Stripe webhook ignored. Event already processed. EventId: {EventId}",
                stripeEvent.Id);

            return BaseResponse.Ok("Event already processed.");
        }

        if (stripeEvent.Type == "payment_intent.succeeded")
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;

            if (paymentIntent is null)
            {
                _logger.LogWarning("Stripe webhook failed. PaymentIntent payload is null.");
                return BaseResponse.Fail("Invalid payment intent payload.");
            }

            var payment = await _paymentRepository.GetByProviderPaymentIntentIdAsync(
                paymentIntent.Id,
                cancellationToken);

            if (payment is null)
            {
                _logger.LogWarning(
                    "Stripe webhook failed. Payment not found. ProviderPaymentIntentId: {ProviderPaymentIntentId}",
                    paymentIntent.Id);

                return BaseResponse.Fail("Payment not found.");
            }

            if (payment.Status == PaymentStatus.Succeeded)
            {
                _logger.LogInformation(
                    "Stripe webhook ignored. Payment already succeeded. PaymentId: {PaymentId}, ProviderPaymentIntentId: {ProviderPaymentIntentId}",
                    payment.Id,
                    payment.ProviderPaymentIntentId);

                await MarkEventAsProcessedAsync(stripeEvent.Id, cancellationToken);

                return BaseResponse.Ok("Payment already processed.");
            }

            var seatHold = await _seatHoldRepository.GetByIdAsync(payment.SeatHoldId, cancellationToken);
            if (seatHold is null)
            {
                _logger.LogWarning(
                    "Stripe webhook failed. Seat hold not found. SeatHoldId: {SeatHoldId}, PaymentId: {PaymentId}",
                    payment.SeatHoldId,
                    payment.Id);

                return BaseResponse.Fail("Seat hold not found.");
            }

            if (seatHold.Status == SeatHoldStatus.Released || seatHold.Status == SeatHoldStatus.Expired)
            {
                payment.Status = PaymentStatus.Failed;

                await _paymentRepository.UpdateAsync(payment, cancellationToken);
                await _paymentRepository.SaveChangesAsync(cancellationToken);
                await _cacheService.RemoveAsync($"payment-status-seatHold:{payment.SeatHoldId}", cancellationToken);

                await MarkEventAsProcessedAsync(stripeEvent.Id, cancellationToken);

                _logger.LogWarning(
                    "Stripe webhook failed. Seat hold is no longer valid. SeatHoldId: {SeatHoldId}, Status: {Status}",
                    seatHold.Id,
                    seatHold.Status);

                return BaseResponse.Fail("Seat hold is no longer valid.");
            }

            var ticketExists = await _ticketRepository.ExistsByScreeningAndSeatAsync(
                seatHold.ScreeningId,
                seatHold.SeatId,
                cancellationToken);

            payment.Status = PaymentStatus.Succeeded;
            payment.PaidAtUtc = DateTime.UtcNow;

            if (seatHold.Status == SeatHoldStatus.Active)
            {
                seatHold.Status = SeatHoldStatus.Purchased;
            }

            await _paymentRepository.UpdateAsync(payment, cancellationToken);
            await _seatHoldRepository.UpdateAsync(seatHold, cancellationToken);
            await _paymentRepository.SaveChangesAsync(cancellationToken);

            if (!ticketExists)
            {
                var ticket = new Ticket
                {
                    ScreeningId = seatHold.ScreeningId,
                    SeatId = seatHold.SeatId,
                    UserId = seatHold.UserId,
                    Price = payment.Amount,
                    Status = TicketStatus.Paid,
                    PurchasedAtUtc = DateTime.UtcNow
                };

                try
                {
                    await _ticketRepository.AddAsync(ticket, cancellationToken);
                    await _ticketRepository.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation(
                        "Ticket created successfully. ScreeningId: {ScreeningId}, SeatId: {SeatId}, UserId: {UserId}",
                        ticket.ScreeningId,
                        ticket.SeatId,
                        ticket.UserId);
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Ticket creation skipped due to duplicate constraint. ScreeningId: {ScreeningId}, SeatId: {SeatId}",
                        seatHold.ScreeningId,
                        seatHold.SeatId);
                }
            }
            else
            {
                _logger.LogWarning(
                    "Ticket already exists for ScreeningId: {ScreeningId}, SeatId: {SeatId}",
                    seatHold.ScreeningId,
                    seatHold.SeatId);
            }

            await _cacheService.RemoveAsync($"payment-status-seatHold:{payment.SeatHoldId}", cancellationToken);
            await MarkEventAsProcessedAsync(stripeEvent.Id, cancellationToken);

            _logger.LogInformation(
                "Stripe payment processed successfully. PaymentId: {PaymentId}, SeatHoldId: {SeatHoldId}, EventId: {EventId}",
                payment.Id,
                seatHold.Id,
                stripeEvent.Id);

            return BaseResponse.Ok("Payment processed successfully.");
        }

        if (stripeEvent.Type == "payment_intent.payment_failed")
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;

            if (paymentIntent is null)
            {
                _logger.LogWarning("Stripe payment_failed webhook received with null PaymentIntent.");
                return BaseResponse.Fail("Invalid payment intent payload.");
            }

            var payment = await _paymentRepository.GetByProviderPaymentIntentIdAsync(
                paymentIntent.Id,
                cancellationToken);

            if (payment is null)
            {
                _logger.LogWarning(
                    "Stripe payment_failed webhook ignored. Payment not found. ProviderPaymentIntentId: {ProviderPaymentIntentId}",
                    paymentIntent.Id);

                return BaseResponse.Fail("Payment not found.");
            }

            if (payment.Status == PaymentStatus.Failed)
            {
                _logger.LogInformation(
                    "Stripe payment_failed webhook ignored. Payment already marked failed. PaymentId: {PaymentId}",
                    payment.Id);

                await MarkEventAsProcessedAsync(stripeEvent.Id, cancellationToken);

                return BaseResponse.Ok("Payment already marked as failed.");
            }

            payment.Status = PaymentStatus.Failed;

            await _paymentRepository.UpdateAsync(payment, cancellationToken);
            await _paymentRepository.SaveChangesAsync(cancellationToken);
            await _cacheService.RemoveAsync($"payment-status-seatHold:{payment.SeatHoldId}", cancellationToken);

            await MarkEventAsProcessedAsync(stripeEvent.Id, cancellationToken);

            _logger.LogInformation(
                "Stripe payment marked as failed. PaymentId: {PaymentId}, ProviderPaymentIntentId: {ProviderPaymentIntentId}, EventId: {EventId}",
                payment.Id,
                payment.ProviderPaymentIntentId,
                stripeEvent.Id);

            return BaseResponse.Ok("Payment marked as failed.");
        }

        await MarkEventAsProcessedAsync(stripeEvent.Id, cancellationToken);

        _logger.LogInformation(
            "Stripe webhook ignored. Unsupported event type: {EventType}, EventId: {EventId}",
            stripeEvent.Type,
            stripeEvent.Id);

        return BaseResponse.Ok("Event ignored.");
    }

    private async Task MarkEventAsProcessedAsync(string eventId, CancellationToken cancellationToken)
    {
        try
        {
            await _processedWebhookEventRepository.AddAsync(
                new ProcessedWebhookEvent
                {
                    EventId = eventId,
                    ProcessedAtUtc = DateTime.UtcNow
                },
                cancellationToken);

            await _processedWebhookEventRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(
                ex,
                "Processed webhook event insert skipped due to duplicate EventId. EventId: {EventId}",
                eventId);
        }
    }
}
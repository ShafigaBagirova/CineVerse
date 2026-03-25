using Application.Common.Interfaces;
using Application.Common.Options;
using Application.Common.Responses;
using Domain.Entities;
using Domain.Enums;
using MediatR;
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
    private readonly ILogger<ProcessStripeWebhookCommandHandler> _logger;
    private readonly StripeSettings _stripeSettings;

    public ProcessStripeWebhookCommandHandler(
        IPaymentRepository paymentRepository,
        ISeatHoldRepository seatHoldRepository,
        ITicketRepository ticketRepository,
        ILogger<ProcessStripeWebhookCommandHandler> logger,
        IOptions<StripeSettings> stripeSettings)
    {
        _paymentRepository = paymentRepository;
        _seatHoldRepository = seatHoldRepository;
        _ticketRepository = ticketRepository;
        _logger = logger;
        _stripeSettings = stripeSettings.Value;
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
                seatHold.Status = SeatHoldStatus.Purchased;

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

                await _ticketRepository.AddAsync(ticket, cancellationToken);
                await _paymentRepository.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Ticket prepared for creation. ScreeningId: {ScreeningId}, SeatId: {SeatId}, UserId: {UserId}",
                    ticket.ScreeningId,
                    ticket.SeatId,
                    ticket.UserId);
            }
            else
            {
                _logger.LogWarning(
                    "Ticket already exists for ScreeningId: {ScreeningId}, SeatId: {SeatId}",
                    seatHold.ScreeningId,
                    seatHold.SeatId);
            }

            await _ticketRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Stripe payment processed successfully. PaymentId: {PaymentId}, SeatHoldId: {SeatHoldId}",
                payment.Id,
                seatHold.Id);

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
            Console.WriteLine($"WEBHOOK PI: {paymentIntent.Id}");

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
            

            payment.Status = PaymentStatus.Failed;

            await _paymentRepository.UpdateAsync(payment, cancellationToken);
            await _paymentRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Stripe payment marked as failed. PaymentId: {PaymentId}, ProviderPaymentIntentId: {ProviderPaymentIntentId}",
                payment.Id,
                payment.ProviderPaymentIntentId);

            return BaseResponse.Ok("Payment marked as failed.");
        }

        _logger.LogInformation(
            "Stripe webhook ignored. Unsupported event type: {EventType}",
            stripeEvent.Type);

        return BaseResponse.Ok("Event ignored.");
    }
}
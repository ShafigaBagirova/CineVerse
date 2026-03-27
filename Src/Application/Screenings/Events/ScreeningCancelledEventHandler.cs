using Application.Common.Interfaces;
using Application.Common.Responses;
using Application.Screenings.Commands;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Screenings.Events;

public sealed class ScreeningCancelledEventHandler
    : INotificationHandler<ScreeningCancelledEvent>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<ScreeningCancelledEventHandler> _logger;
    private readonly IStripeService _stripeService;
    private readonly IUserNotificationService _userNotificationService;
    public ScreeningCancelledEventHandler(
        ITicketRepository ticketRepository,
        IPaymentRepository paymentRepository,
        ILogger<ScreeningCancelledEventHandler> logger,
        IStripeService stripeService,
        IUserNotificationService userNotificationService)
    {
        _ticketRepository = ticketRepository;
        _paymentRepository = paymentRepository;
        _logger = logger;
        _stripeService = stripeService;
        _userNotificationService = userNotificationService;
    }

    public async Task Handle(ScreeningCancelledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Refund process started. ScreeningId: {ScreeningId}",
            notification.ScreeningId);

        var tickets = await _ticketRepository.GetPaidTicketsByScreeningIdAsync(
            notification.ScreeningId,
            cancellationToken);

        foreach (var ticket in tickets)
        {
            try
            {
                var payment = await _paymentRepository.GetSucceededByScreeningAndSeatAsync(
                    ticket.ScreeningId,
                    ticket.SeatId,
                    cancellationToken);

                if (payment is null)
                {
                    _logger.LogWarning(
                        "Refund skipped. Payment not found. TicketId: {TicketId}, ScreeningId: {ScreeningId}, SeatId: {SeatId}",
                        ticket.Id,
                        ticket.ScreeningId,
                        ticket.SeatId);
                    continue;
                }

                if (payment.Status == PaymentStatus.Refunded)
                {
                    _logger.LogInformation(
                        "Refund skipped. Payment already refunded. PaymentId: {PaymentId}",
                        payment.Id);
                    continue;
                }

                if (payment.Status != PaymentStatus.Succeeded)
                {
                    _logger.LogInformation(
                        "Refund skipped. Payment is not succeeded. PaymentId: {PaymentId}, Status: {Status}",
                        payment.Id,
                        payment.Status);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(payment.ProviderPaymentIntentId))
                {
                    _logger.LogWarning(
                        "Refund skipped. ProviderPaymentIntentId is empty. PaymentId: {PaymentId}",
                        payment.Id);
                    continue;
                }

                var idempotencyKey = $"refund-cancelled-screening-payment-{payment.Id}";

                await _stripeService.CreateRefundAsync(
                    payment.ProviderPaymentIntentId,
                    idempotencyKey,
                    cancellationToken);

                payment.Status = PaymentStatus.Refunded;
                payment.RefundedAtUtc = DateTime.UtcNow;

                ticket.Status = TicketStatus.Cancelled;

                await _paymentRepository.UpdateAsync(payment, cancellationToken);
                await _ticketRepository.UpdateAsync(ticket, cancellationToken);

                await _paymentRepository.SaveChangesAsync(cancellationToken);
                try
                {
                    await _userNotificationService.SendScreeningCancelledEmailAsync(
                        ticket.UserId,
                        ticket.ScreeningId,
                        cancellationToken);

                    await _userNotificationService.SendRefundCompletedEmailAsync(
                        ticket.UserId,
                        ticket.ScreeningId,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Cancelled screening refund email sending failed. TicketId: {TicketId}, UserId: {UserId}",
                        ticket.Id,
                        ticket.UserId);
                }
                _logger.LogInformation(
                    "Refund success. PaymentId: {PaymentId}, TicketId: {TicketId}",
                    payment.Id,
                    ticket.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Refund failed. TicketId: {TicketId}",
                    ticket.Id);
            }
        }

        _logger.LogInformation(
            "Refund process completed. ScreeningId: {ScreeningId}",
            notification.ScreeningId);
    }
}
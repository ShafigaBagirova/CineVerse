using Application.Common.Interfaces;
using Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Screenings.Events;

public sealed class ScreeningRescheduledEventHandler
    : INotificationHandler<ScreeningRescheduledEvent>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<ScreeningRescheduledEventHandler> _logger;
    private readonly IStripeService _stripeService;
    private readonly IUserNotificationService _userNotificationService;

    public ScreeningRescheduledEventHandler(
        ITicketRepository ticketRepository,
        IPaymentRepository paymentRepository,
        ILogger<ScreeningRescheduledEventHandler> logger,
        IStripeService stripeService,
        IUserNotificationService userNotificationService)
    {
        _ticketRepository = ticketRepository;
        _paymentRepository = paymentRepository;
        _logger = logger;
        _stripeService = stripeService;
        _userNotificationService = userNotificationService;
    }

    public async Task Handle(
        ScreeningRescheduledEvent notification,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "ScreeningRescheduledEventHandler started. ScreeningId: {ScreeningId}",
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

                var idempotencyKey = $"refund-rescheduled-screening-payment-{payment.Id}";

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
                    await _userNotificationService.SendScreeningRescheduledEmailAsync(
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
                        "Rescheduled screening refund email sending failed. TicketId: {TicketId}, UserId: {UserId}",
                        ticket.Id,
                        ticket.UserId);
                }
                _logger.LogInformation(
                    "Refund completed successfully. ScreeningId: {ScreeningId}, TicketId: {TicketId}, PaymentId: {PaymentId}",
                    notification.ScreeningId,
                    ticket.Id,
                    payment.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Refund failed for rescheduled screening. ScreeningId: {ScreeningId}, TicketId: {TicketId}",
                    notification.ScreeningId,
                    ticket.Id);
            }
        }

        _logger.LogInformation(
            "ScreeningRescheduledEventHandler completed. ScreeningId: {ScreeningId}",
            notification.ScreeningId);
    }
}
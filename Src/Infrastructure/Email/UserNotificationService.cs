using Application.Common.Interfaces;
using Application.Common.Dtos;

namespace Infrastructure.Email;

public class UserNotificationService:IUserNotificationService
{
    private readonly IEmailSender _emailSender;
    private readonly IUserEmailProvider _userEmailProvider;

    public UserNotificationService(
        IEmailSender emailSender,
        IUserEmailProvider userRepository)
    {
        _emailSender = emailSender;
        _userEmailProvider = userRepository;
    }

    private async Task<string?> GetUserEmailAsync(string userId, CancellationToken cancellationToken)
    {
      return await _userEmailProvider.GetEmailByUserIdAsync(userId, cancellationToken);
       
    }

    public async Task SendPaymentSucceededEmailAsync(
        string userId,
        PaymentSuccessNotificationDto notification,
        CancellationToken cancellationToken)
    {
        var email = await GetUserEmailAsync(userId, cancellationToken);
        if (email is null) return;

        var subject = "Payment Successful 🎉";
        var timeText = FormatScreeningTimeForDisplay(notification.StartTime);

        var body = $"""
            Your ticket is confirmed!

            Movie: {notification.MovieTitle}
            Cinema: {notification.CinemaName}
            Hall: {notification.HallName}
            Time: {timeText}
            Seat: Row {notification.SeatRow}, Seat {notification.SeatNumber}

            Enjoy your movie!
            """;

        await _emailSender.SendAsync(email, subject, body);
    }

    private static string FormatScreeningTimeForDisplay(DateTime startTime)
    {
        // If already local/unspecified cinema time, do not convert again.
        if (startTime.Kind != DateTimeKind.Utc)
        {
            return startTime.ToString("hh:mm tt");
        }

        TimeZoneInfo bakuTimeZone;
        try
        {
            bakuTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Azerbaijan Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            bakuTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Baku");
        }

        var localStartTime = TimeZoneInfo.ConvertTimeFromUtc(startTime, bakuTimeZone);
        return localStartTime.ToString("hh:mm tt");
    }

    public async Task SendPaymentFailedEmailAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var email = await GetUserEmailAsync(userId, cancellationToken);
        if (email is null) return;

        var subject = "Payment Failed ❌";

        var body = """
            Your payment attempt has failed.

            Please try again before your seat hold expires.
            """;

        await _emailSender.SendAsync(email, subject, body);
    }

    public async Task SendScreeningCancelledEmailAsync(
        string userId,
        int screeningId,
        CancellationToken cancellationToken)
    {
        var email = await GetUserEmailAsync(userId, cancellationToken);
        if (email is null) return;

        var subject = "Screening Cancelled";

        var body = $"""
            The screening you booked has been cancelled by the cinema.

            Screening ID: {screeningId}

            Your payment will be refunded.
            """;

        await _emailSender.SendAsync(email, subject, body);
    }

    public async Task SendScreeningRescheduledEmailAsync(
        string userId,
        int screeningId,
        CancellationToken cancellationToken)
    {
        var email = await GetUserEmailAsync(userId, cancellationToken);
        if (email is null) return;

        var subject = "Screening Rescheduled";

        var body = $"""
            Your screening has been rescheduled by the cinema.

            Screening ID: {screeningId}

            Your payment will be refunded.
            """;

        await _emailSender.SendAsync(email, subject, body);
    }

    public async Task SendRefundCompletedEmailAsync(
        string userId,
        int screeningId,
        CancellationToken cancellationToken)
    {
        var email = await GetUserEmailAsync(userId, cancellationToken);
        if (email is null) return;

        var subject = "Refund Completed 💰";

        var body = $"""
            Your refund has been successfully processed.

            Screening ID: {screeningId}

            The amount will be returned to your payment method shortly.
            """;

        await _emailSender.SendAsync(email, subject, body);
    }
}

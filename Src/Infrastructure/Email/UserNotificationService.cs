using Application.Common.Interfaces;

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
        int screeningId,
        CancellationToken cancellationToken)
    {
        var email = await GetUserEmailAsync(userId, cancellationToken);
        if (email is null) return;

        var subject = "Payment Successful 🎉";

        var body = $"""
            Your payment was completed successfully.

            Screening ID: {screeningId}

            Enjoy your movie!
            """;

        await _emailSender.SendAsync(email, subject, body);
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

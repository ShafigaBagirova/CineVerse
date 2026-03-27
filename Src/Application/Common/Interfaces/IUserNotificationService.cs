namespace Application.Common.Interfaces;

public interface IUserNotificationService
{
    Task SendPaymentSucceededEmailAsync(string userId,int screeningId,CancellationToken cancellationToken);

    Task SendPaymentFailedEmailAsync(string userId,CancellationToken cancellationToken);
    Task SendScreeningCancelledEmailAsync(string userId, int screeningId, CancellationToken cancellationToken);

    Task SendScreeningRescheduledEmailAsync( string userId,int screeningId,CancellationToken cancellationToken);

    Task SendRefundCompletedEmailAsync(string userId,int screeningId, CancellationToken cancellationToken);
}
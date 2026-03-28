namespace Application.Common.Helpers;

public static class PaymentCacheKey
{
    public const string GetPaymentsBySeatHoldPrefix = "payments:seathold:";
    public const string GetPaymentByIdPrefix = "payment:id:";
    public const string BySeatHoldPrefix = "payment-status-seatHold:";
    public const string AllPrefix = "payments:";

    public static string GetBySeatHoldId(int seatHoldId) => $"{BySeatHoldPrefix}{seatHoldId}";
}
namespace Application.Common.Options;

public sealed class VipPaymentOptions
{
    public const string SectionName = "Vip";

    public decimal MonthlyPriceAzn { get; set; } = 9.99m;
}

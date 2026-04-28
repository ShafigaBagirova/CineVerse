namespace Application.Payments.Dtos;

public sealed class CreateVipPaymentIntentResponse
{
    public string ClientSecret { get; set; } = default!;
    public string ProviderPaymentIntentId { get; set; } = default!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "azn";
}
